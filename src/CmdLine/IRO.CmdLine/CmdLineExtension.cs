using IRO.CmdLine.Json;
using IRO.Reflection.Core;
using IRO.Storage;
using System;
using System.Reflection;
using IRO.Storage.DefaultStorages;
using System.IO;
using IRO.Storage.Data;

namespace IRO.CmdLine
{
    /// <summary>
    /// Класс-адаптер для класса Console. Добавляет некоторые крутые фичи и еще я его использую для абстракции от консоли, на случай если захочу сделать веб версию консоли.
    /// </summary>
    public class CmdLineExtension
    {
        public IKeyValueStorage Storage { get; }

        public IConsoleHandler ConsoleHandler { get; }

        /// <summary>
        /// Для сложных методов, типа считывания сложных типов через json editor вы можете отключить исключения. Тогда, к примеру, если пользователь допустит 
        /// ошибку при редактировании json файла - ему выведут сообщение, но исключение не завершит работу программы.
        /// </summary>
        public bool ThrowConsoleParseExeptions { get; } = false;

        public CmdLineExtension(IConsoleHandler consoleHandler, IKeyValueStorage storage = null)
        {
            var opt = new FileStorageInitOptions()
            {
                StorageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CmdLineStorage.json")
            };
            Storage = storage ?? new FileStorage(opt);
            if (consoleHandler == null)
                throw new NullReferenceException();
            ConsoleHandler = consoleHandler;
        }

        public void WriteLine()
        {
            ConsoleHandler.WriteLine();
        }

        public string ReadLine()
        {
            return ConsoleHandler.ReadLine();
        }

        /// <summary>
        /// Отличается от стандартного метода Console тем, что если передаваемый объект не IConvertible, то он будет сериализирован в json строку.
        /// </summary>
        public void WriteLine(object objToWrite, ConsoleColor? cc = null)
        {
            ConsoleHandler.WriteLine(
                JsonSerializeHelper.Inst.ToConvertibleOrJson(objToWrite),
                cc
                );
        }


        /// <summary>
        /// Отличается от стандартного метода Console тем, что если передаваемый объект не IConvertible, то он будет сериализирован в json строку.
        /// </summary>
        public void Write(object objToWrite, ConsoleColor? cc = null)
        {
            ConsoleHandler.Write(
                JsonSerializeHelper.Inst.ToConvertibleOrJson(objToWrite),
                cc
                );
        }

        /// <summary>
        /// Запрашивайте данные от пользователя ТОЛЬКО ЧЕРЕЗ ЭТОТ МЕТОД.
        /// Если запрашиваемый тип реализует IConvertible, то он будет запрошен через консоль. Иначе - пользователю дадут отредактировать Json файл.
        /// <para></para>
        /// Еще этот метод может кешировать значение по имени ресурса, используя его можно с легкостью реализовать астозаполнение и даже автоматическое тестирование.
        /// </summary>
        public T ReadResource<T>(string resourceName, ReadResourseOptions options = null)
        {
            return (T)readResource(typeof(T), 10, resourceName, options, default(T));
        }

        /// <summary>
        /// Запрашивайте данные от пользователя ТОЛЬКО ЧЕРЕЗ ЭТОТ МЕТОД.
        /// Если запрашиваемый тип реализует IConvertible, то он будет запрошен через консоль. Иначе - пользователю дадут отредактировать Json файл.
        /// <para></para>
        /// Еще этот метод может кешировать значение по имени ресурса, используя его можно с легкостью реализовать астозаполнение и даже автоматическое тестирование.
        /// </summary>
        public object ReadResource(Type typeOfResource, string resourceName, ReadResourseOptions options = null)
        {            
            return readResource(typeOfResource, 10, resourceName, options, null);
        }

        object readResource(Type objectType, int tryesCount, string resourceName, ReadResourseOptions options, object defaultValue)
        {
            if (options == null)
                options = new ReadResourseOptions();
            var longResName = resourceName + "";
            try
            {
                WriteLine($"Resource '{longResName}' with type " +
                    $"{objectType.GetNormalTypeName(false)} requested.", ConsoleColor.Yellow);               

                string cachedValueString = Storage.GetOrDefault<string>(longResName).Result;

                if (typeof(IConvertible).IsAssignableFrom(objectType))
                {
                    return IfResourceIsIConvertible(objectType,longResName, cachedValueString, options);
                }
                else
                {
                    return IfResourceIsDifficult(objectType,longResName, cachedValueString, defaultValue, options);
                }

            }
            catch (Exception ex)
            {
                if (ThrowConsoleParseExeptions || tryesCount < 0)
                {
                    throw;
                }
                else
                {
                    WriteLine("Exeption in console resource receiving method: ", ConsoleColor.DarkRed);
                    WriteLine(("\t" + ex.Message).Replace("\n", "\n\t"));

                    //try again
                    return readResource(objectType, tryesCount - 1, resourceName, options, defaultValue);
                }
            }
        }

        object IfResourceIsDifficult(Type objectType, string longResName, string cachedValueString, object defaultValue, ReadResourseOptions options)
        {
            //Else, will be converted by json.
            //
            WriteLine(
                $"Difficult type. Will be opened in json editor. ",
                ConsoleColor.Yellow
                );            

            object cachedValue;
            try
            {
                if (defaultValue == null)
                {
                    cachedValue = JsonSerializeHelper.Inst.FromJson(objectType,cachedValueString);
                }
                else
                {
                    cachedValue = defaultValue;
                }
            }
            catch
            {
                cachedValue = ReflectionHelpers.CreateTypeExample(objectType);
            }

            ReadLine();
            
            bool isAccept;
            string editedJson = null;
            string jsonPrototypeString = JsonSerializeHelper.Inst.ToJson(
                objectType,
                cachedValue
                );
            do
            {
                editedJson = ConsoleHandler.ReadJson(jsonPrototypeString);               

                Write("Accept changes? Press y/n (y): ", ConsoleColor.Yellow);
                isAccept = ReadLine().Trim().StartsWith("n");
            } while (isAccept);
             

            object res = JsonSerializeHelper.Inst.FromJson(objectType,editedJson);

            //Convert again to normal parse json.
            if(options.SaveToCache)
                Storage.Set(longResName, JsonSerializeHelper.Inst.ToJson(objectType, res));
            return res;
            //
            //Else, will be converted by json.
        }

        object IfResourceIsIConvertible(Type objectType, string longResName,string cachedValueString,ReadResourseOptions options)
        {
            //If IConvertible
            //
            string cachedValue = cachedValueString;
            var cachedValueInHint = cachedValue ?? "";
            if (cachedValueInHint.Length > 80)
            {
                cachedValueInHint = cachedValueInHint.Substring(0, 80) + "... ";
            }

            Write(
                $"Input ({cachedValueInHint}): ",
                ConsoleColor.Yellow
                );
            
            //Автоматическое считывание если пустая строка.
            string val = ReadLine().Trim();
            if (val=="" && cachedValue != null)
            {
                val = cachedValue;
            }
            else
            {
                if (options.SaveToCache)
                    Storage.Set(longResName, val);
            }


            object res=null;
            if (objectType == typeof(bool) || objectType == typeof(bool?))
            {
                val = val.Trim();
                if (val == "y")
                    res = true;
                if (val == "n")
                    res = false;

            }
            if (res == null)
                res = Convert.ChangeType(val, objectType);
            return res;
            //
            //If IConvertible
        }

        static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);

            return null;
        }
    }
}
