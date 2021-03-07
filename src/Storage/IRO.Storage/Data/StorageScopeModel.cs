using System.Collections.Generic;
using Newtonsoft.Json;

namespace IRO.Storage.DefaultStorages
{
    internal class StorageScopeModel
    {
        /// <summary>
        /// Always true, sets ignored.
        /// </summary>
        [JsonProperty(nameof(IsStorageScopeModel))]
        public bool IsStorageScopeModel 
        {
            get => true;
            // ReSharper disable once ValueParameterNotUsed
            internal set { }
        }

        public HashSet<string> ScopeKeys { get; set; } = new HashSet<string>();

        public void AddKey(string key)
        {
            if (ScopeKeys.Contains(key))
                return;
            ScopeKeys.Add(key);
        }

        public void RemoveKey(string key)
        {
            if (!ScopeKeys.Contains(key))
                return;
            ScopeKeys.Remove(key);
        }
    }
}