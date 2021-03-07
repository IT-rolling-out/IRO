using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IRO.Storage.Data;
using IRO.Storage.Exceptions;
using NeoSmart.AsyncLock;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IRO.Storage.DefaultStorages
{
    /// <summary>
    /// Base class for all <see cref="IKeyValueStorage"/> implementions.
    /// </summary>
    public abstract class BaseStorage : IKeyValueStorage
    {
        public const char ScopeSplitter = '.';

        readonly AsyncLock _lock = new AsyncLock();

        public async Task Clear()
        {
            await UsingLock(async () =>
            {
                await InnerClear();
            });
        }

        public async Task<bool> ContainsKey(string key)
        {
            ThrowIfBadKey(key);
            return await UsingLock(async () =>
            {
                var str = await InnerGet(key);
                return str != null;
            });
        }

        public async Task<object> Get(Type type, string key)
        {
            return await UsingLock(async () =>
            {
                var jToken = await Get(key);
                return jToken?.ToObject(type);
            });
        }

        public async Task<JToken> Get(string key)
        {
            ThrowIfBadKey(key);
            if (!await ContainsKey(key))
            {
                throw new Exception($"Storage not contains key '{key}'");
            }
            return await UsingLock(async () =>
            {
                var jToken = await GetAndParseToJToken(key);
                if (IsScopeModel(jToken))
                {
                    var scopeCasted = jToken.ToObject<StorageScopeModel>();
                    var scopeWithValues = new JObject();
                    foreach (var innerKey in scopeCasted.ScopeKeys)
                    {
                        var fullInnerKey = $"{key}{ScopeSplitter}{innerKey}";
                        var val = await Get(fullInnerKey);
                        scopeWithValues[innerKey] = val;
                    }
                    return scopeWithValues;
                }
                else
                {
                    return jToken;
                }
            });
        }

        public async Task Set(string key, object value)
        {
            await Remove(key);

            ThrowIfBadKey(key);
            await UsingLock(async () =>
            {
                var isScope = key.Contains(ScopeSplitter);
                if (isScope)
                {
                    var keyValuesToSave = new Dictionary<string, object>();

                    var scopesHierarchy = key.Split(ScopeSplitter);
                    var toNum = scopesHierarchy.Length - 1;
                    var scopeName = "";
                    for (var i = 0; i < toNum; i++)
                    {
                        try
                        {
                            scopeName += scopesHierarchy[i];
                            var scope = await GetScopeFromStorageOrCreate(scopeName);
                            scope.AddKey(scopesHierarchy[i + 1]);
                            keyValuesToSave[scopeName] = scope;
                            scopeName += ScopeSplitter;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }

                    //Save all.
                    foreach (var item in keyValuesToSave)
                    {
                        await SetObject(item.Key, item.Value);
                    }
                }

                await SetObject(key, value);

            });
        }

        public async Task Remove(string key)
        {
            ThrowIfBadKey(key);
            await UsingLock(async () =>
            {
                var keyValuesToSave = new Dictionary<string, object>();
                await RemoveScopeChildren(key, keyValuesToSave);
                await RemoveObjectFromParentScopes(key, keyValuesToSave);

                //Remove upper scopes if empty and value at this point.
                foreach (var item in keyValuesToSave)
                {
                    if (item.Value == null)
                    {
                        await InnerRemove(item.Key);
                    }
                    else
                    {
                        await SetObject(item.Key, item.Value);
                    }
                }
                await InnerRemove(key);

            });
        }

        #region Protected abstract.
        /// <summary>
        /// Return string value or null if not contains (or removed).
        /// </summary>
        protected abstract Task<string> InnerGet(string key);

        protected abstract Task InnerSet(string key, string strValue);

        /// <summary>
        /// Removed keys return null, just like undefined keys do.
        /// </summary>
        protected abstract Task InnerRemove(string key);

        protected abstract Task InnerClear();
        #endregion

        #region Private.

        /// <summary>
        /// Not directly apply changes, just save them to buffer dict.
        /// </summary>
        async Task RemoveScopeChildren(string key, IDictionary<string, object> keyValuesToSave)
        {
            var jToken = await GetAndParseToJToken(key);
            if (IsScopeModel(jToken))
            {
                var scopeCasted = jToken.ToObject<StorageScopeModel>();
                foreach (var innerKey in scopeCasted.ScopeKeys)
                {
                    var fullInnerKey = $"{key}{ScopeSplitter}{innerKey}";
                    keyValuesToSave[fullInnerKey] = null;
                    await RemoveScopeChildren(fullInnerKey, keyValuesToSave);
                }
            }
        }

        /// <summary>
        /// Not directly apply changes, just save them to buffer dict.
        /// </summary>
        async Task RemoveObjectFromParentScopes(string key, IDictionary<string, object> keyValuesToSave)
        {
            var (scopeFullName, innerKeyName) = RemoveLastScope(key);
            bool removeFromParentScope = true;
            while (scopeFullName != null)
            {
                try
                {
                    var scope = await GetScopeFromStorageOrCreate(scopeFullName);
                    if (removeFromParentScope)
                    {
                        scope.RemoveKey(innerKeyName);
                        removeFromParentScope = !scope.ScopeKeys.Any();
                    }

                    //Remove scope if null.
                    keyValuesToSave[scopeFullName] = scope.ScopeKeys.Any() ? scope : null;

                    (scopeFullName, innerKeyName) = RemoveLastScope(scopeFullName);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }



        (string ScopeFullName, string InnerKeyName) RemoveLastScope(string key)
        {
            var index = key.LastIndexOf(ScopeSplitter);
            if (index <= 0)
                return (null, key);
            var scopeFullName = key.Remove(index);
            var innerKeyName = key.Substring(index + 1);
            return (scopeFullName, innerKeyName);
        }


        void ThrowIfBadKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Bad key.", nameof(key));
            }
        }

        bool IsScopeModel(JToken jToken)
        {
            try
            {
                if (jToken == null)
                    return false;
                return jToken[nameof(StorageScopeModel.IsStorageScopeModel)].ToObject<bool>();
            }
            catch
            {
                return false;
            }
        }

        async Task<T> UsingLock<T>(Func<Task<T>> func)
        {
            using (await _lock.LockAsync())
            {
                try
                {
                    return await func();
                }
                catch (Exception ex)
                {
                    throw new StorageException("", ex);
                }
            }
        }

        async Task UsingLock(Func<Task> func)
        {
            using (await _lock.LockAsync())
            {
                try
                {
                    await func();
                }
                catch (Exception ex)
                {
                    throw new StorageException("", ex);
                }
            }
        }

        async Task<JToken> GetAndParseToJToken(string key)
        {
            var jsonStr = await InnerGet(key);

            //When not contains.
            if (jsonStr == null)
            {
                return null;
            }

            //When was set value 'null'.
            if (jsonStr.Trim() == "null")
            {
                return null;
            }
            return JToken.Parse(jsonStr);
        }

        async Task SetObject(string key, object value)
        {
            var jsonStr = JsonConvert.SerializeObject(value);
            await InnerSet(key, jsonStr);
        }

        async Task<StorageScopeModel> GetScopeFromStorageOrCreate(string scopeName)
        {
            var jToken = await GetAndParseToJToken(scopeName);
            if (IsScopeModel(jToken))
            {
                return jToken.ToObject<StorageScopeModel>();
            }
            return new StorageScopeModel();

        }
        #endregion
    }
}