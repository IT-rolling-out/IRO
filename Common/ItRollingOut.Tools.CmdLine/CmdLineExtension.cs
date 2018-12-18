using ItRollingOut.Tools.CmdLine.Json;
using ItRollingOut.Tools.Reflection;
using ItRollingOut.Tools.Storage;
using System;
using System.Reflection;

namespace ItRollingOut.Tools.CmdLine
{
    /// <summary>
    /// Класс-адаптер для класса Console. Добавляет некоторые крутые фичи и еще я его использую для абстракции от консоли, на случай если захочу сделать веб версию консоли.
    /// </summary>
    public class CmdLineExtension
    {
        #region Static part
        static CmdLineExtension _inst;
        public static CmdLineExtension Inst
        {
            get
            {
                if (_inst == null)
                {
                    throw new Exception("Before use CmdLineExtension.Inst you must init it.");
                }
                return _inst;
            }
        }

        /// <summary>
        /// Init singleton console. You can access it through CmdLineExtension.Inst .
        /// </summary>
        /// <param name="consoleHandler">If null, will use default handler.</param>
        public static void Init(IConsoleHandler consoleHandler=null)
        {
            if (_inst != null)
            {
                throw new Exception("Console was inited before.");
            }
            _inst = new CmdLineExtension(
                consoleHandler ?? new DefaultConsoleHandler()
                );
        }
        #endregion       

        public IConsoleHandler ConsoleHandler { get; private set; }

        /// <summary>
        /// Для сложных методов, типа считывания сложных типов через json editor вы можете отключить исключения. Тогда, к примеру, если пользователь допустит 
        /// ошибку при редактировании json файла - ему выведут сообщение, но исключение не завершит работу программы.
        /// </summary>
        public bool ThrowConsoleParseExeptions { get; } = false;       

        public CmdLineExtension(IConsoleHandler consoleHandler)
        {
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
        public void WriteLine(object objToWrite, ConsoleColor? cc = null, bool prettyJson = false)
        {
            ConsoleHandler.WriteLine(
                JsonSerializeHelper.Inst.ToConvertibleOrJson(objToWrite, new JsonSerializeOptions() { WithNormalFormating = prettyJson }),
                cc
                );
        }


        /// <summary>
        /// Отличается от стандартного метода Console тем, что если передаваемый объект не IConvertible, то он будет сериализирован в json строку.
        /// </summary>
        public void Write(object objToWrite, ConsoleColor? cc = null, bool prettyJson = false)
        {
            ConsoleHandler.Write(
                JsonSerializeHelper.Inst.ToConvertibleOrJson(objToWrite, new JsonSerializeOptions() { WithNormalFormating = prettyJson }),
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

                string cachedValueString = StorageHardDrive.Get<string>(longResName).Result;

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
            this.WriteLine(
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
                ConstructorInfo ctor = objectType.GetConstructor(new Type[] { });
                cachedValue = ctor.Invoke(new object[] { });
            }

            //Если автоматическое считывание, то возвращаем закешированное значение.
            if (options.UseAutoread)
            {
                return cachedValue;
            }

            this.ReadLine();
            
            bool isAccept;
            string editedJson = null;
            string jsonPrototypeString = JsonSerializeHelper.Inst.ToJson(
                objectType,
                cachedValue,
                new JsonSerializeOptions() { WithNormalFormating = true }
                );
            do
            {
                editedJson = ConsoleHandler.ReadJson(jsonPrototypeString);               

                this.Write("Accept changes? Press y/n (y): ", ConsoleColor.Yellow);
                isAccept = this.ReadLine().Trim().StartsWith("n");
            } while (isAccept);
             

            object res = JsonSerializeHelper.Inst.FromJson(objectType,editedJson);

            //Convert again to normal parse json.
            if(options.SaveToCache)
                StorageHardDrive.Set(longResName, JsonSerializeHelper.Inst.ToJson(objectType, res));
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

            this.Write(
                $"Input ({cachedValueInHint}): ",
                ConsoleColor.Yellow
                );

            //Если автоматическое считывание, то возвращаем закешированное значение.
            string val = "";
            if (!options.UseAutoread)
            {
                val= this.ReadLine();
            }

            
            if (val=="" && cachedValue != null)
            {
                val = cachedValue;
            }
            else
            {
                if (options.SaveToCache)
                    StorageHardDrive.Set(longResName, val);
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
