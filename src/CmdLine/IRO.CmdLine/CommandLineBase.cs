using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using IRO.Storage;
using IRO.Common.Services;
using IRO.Common.Text;
using IRO.Reflection.Core.ModelBinders;
using IRO.Reflection.Core;

namespace IRO.CmdLine
{
    public class ResolvedCmdInfo
    {
        public MethodInfo MethodInfo { get; set; }

        public string CmdName { get; set; }

        public string OriginalMethodName { get; set; }

        public string Description { get; set; }
    }

    /// <summary>
    /// Базовый класс для генерации cli.
    /// Можно писать методы с входными параметрами (массив строк).
    /// Вместо Console обязательно используйте this.
    /// </summary>
    public class CommandLineBase
    {
        /// <summary>
        /// Показывает открыта ли сейчас эта консоль в стеке консолей.
        /// </summary>
        public bool IsInRun
        {
            get { return _isInRun; }
            private set
            {
                if (!_isInRun)
                    OnQuit?.Invoke();
                _isInRun = value;

            }
        }

        public CmdLineExtension Cmd { get; private set; }

        public Dictionary<string, ResolvedCmdInfo> CmdNameAndInfo { get; } = new Dictionary<string, ResolvedCmdInfo>();

        public event Action OnQuit;

        CmdSwitcher _currentCmdSwitcher;

        bool _isInRun = true;

        /// <summary>
        /// Используйте это свойство в своих командах, чтоб предложить следующую команду.
        /// </summary>
        public string LastCmdNameAndParams
        {
            get
            {
                return StorageHardDrive.Get<string>(GetType().Name + ".last_cmd_params").Result;
            }
            set
            {
                StorageHardDrive.Set(GetType().Name + ".last_cmd_params", value);
            }
        }

        public CommandLineBase(CmdLineExtension cmdLineExtension = null)
        {
            Cmd = cmdLineExtension ?? CmdLineExtension.Inst;
            CmdNameAndInfo = CreateReflectionDict();
        }

        internal void SetCmdSwitcher(CmdSwitcher cmdSwitcher)
        {
            _currentCmdSwitcher = cmdSwitcher;
        }

        /// <summary>
        /// Open another cmd, current cmd now not available until sub cmd is not closed.
        /// </summary>
        protected void TryOpenSubCmd(CommandLineBase commandLineBase)
        {
            try
            {
                _currentCmdSwitcher.PushCmdInStack(commandLineBase);
            }
            catch { }
        }

        public virtual void OnStart()
        {
            Cmd.WriteLine(
                $"You have been opened command line '{GetType().Name}'. Write 'help' to open commands list.",
                ConsoleColor.Magenta
                );
        }

        [CmdInfo(CmdName = "help")]
        public void HelpCmd()
        {
            var commonInfo = "In methods with parameters you can pass values(or default will be used)." +
                "\nTo define parameter values use json syntax." +
                "\nTo pass parameters you can write something like " +
                "\n -> cmd_name /strParam:\"it`s string\" /boolParam:1 /intParam:6" +
                "\nAlso, you can pass command and parameters in cmd arguments, when start your app. Example:" +
                "\n -> dotnet MyAppFile.dll cmd_name /strParam ..." +
                "\n\nMethods list:\n";
            StringBuilder res = new StringBuilder(commonInfo);
            foreach (var cmdPair in CmdNameAndInfo)
            {
                var cmdInfo = cmdPair.Value;
                if (cmdInfo.CmdName != "help")
                {
                    string newStr = "\t" + cmdInfo.CmdName + " - " + cmdInfo.OriginalMethodName + "(";
                    bool isFirst = true;

                    foreach (var parameter in cmdInfo.MethodInfo.GetParameters())
                    {
                        if (!isFirst)
                        {
                            newStr += ", ";
                        }
                        isFirst = false;
                        newStr += parameter.ParameterType.Name + "  " + parameter.Name;
                    }
                    newStr += ");";

                    if (cmdInfo.Description != null)
                    {
                        var description = cmdInfo.Description
                            .Replace("\n", "\n\t* ");
                        newStr += $"  /*{description}*/";
                    }
                    res.AppendLine(newStr);
                }


            }
            var resStr = res.ToString();//.Replace("\n","\n\t");
            Cmd.Write(resStr);
        }

        public virtual void OnEveryLoop()
        {
            Cmd.Write(
                $"cmd ( { LastCmdNameAndParams ?? ""} ) : ",
                ConsoleColor.DarkGreen
                );

            string cmdName = Cmd.ReadLine();
            ExecuteCmd(cmdName);


            Cmd.WriteLine();

        }

        /// <summary>
        /// Запускает соответствующий метод по названию команды. 
        /// Перехватывает ошибки выполнения.
        /// </summary>
        public void ExecuteCmd(string cmdAndParamsStr)
        {
            if (string.IsNullOrWhiteSpace(cmdAndParamsStr))
            {
                cmdAndParamsStr = LastCmdNameAndParams;
            }
            else
            {
                LastCmdNameAndParams = cmdAndParamsStr;
            }

            try
            {
                var cmdName = ResolveCmdName(cmdAndParamsStr);
                if (CmdNameAndInfo.TryGetValue(cmdName, out var cmdInfo))
                {
                    var currentMethodInfo = cmdInfo.MethodInfo;
                    var parameters = ResolveCmdParams(cmdAndParamsStr, cmdInfo);

                    if (parameters.Length > 0)
                        currentMethodInfo.Invoke(this, parameters);
                    else
                        currentMethodInfo.Invoke(this, new object[0]);
                    LastCmdNameAndParams = cmdAndParamsStr;
                }

                else
                {
                    Cmd.WriteLine("Command not found.");
                }                
            }
            catch (Exception ex)
            {
                Cmd.WriteLine("Executing command throwed exception: " + ex.ToString(), ConsoleColor.DarkRed);
                //ApiException apiEx = ExceptionsHelper.FindInnerExceptionInAggregateException<ApiException>(ex);
                //if (apiEx!=null)
                //{
                //    var apiErr = apiEx.GetApiError();
                //    HandleApiError(apiErr);
                //}
                Cmd.Write("\nWant to ignore it? Press y/n (y): ", ConsoleColor.DarkRed);
                var consoleText = Cmd.ReadLine();
                if (consoleText.Trim() == "")
                {
                    //throw;
                }
                if (consoleText.Trim().StartsWith("n"))
                {
                    throw;
                }
            }
        }

        string ResolveCmdName(string cmdAndParamsStr)
        {
            int firstParamIndex = cmdAndParamsStr.IndexOf('/');
            string cmdName = null;
            if (firstParamIndex > 0)
            {
                cmdName = cmdAndParamsStr.Remove(firstParamIndex).Trim();
            }
            else
            {
                cmdName = cmdAndParamsStr.Trim();
            }
            if (string.IsNullOrWhiteSpace(cmdName))
                throw new Exception($"Wrong cmd name in '{cmdAndParamsStr}'.");

            return cmdName;
        }

        object[] ResolveCmdParams(string cmdAndParamsStr, ResolvedCmdInfo resolvedCmdInfo)
        {

            int firstParamIndex = cmdAndParamsStr.IndexOf('/');
            string paramsStr = null;
            if (firstParamIndex > 0)
            {
                paramsStr = cmdAndParamsStr.Substring(firstParamIndex).Trim();
            }
            else
            {
                paramsStr = "";
            }
            var parameters = resolvedCmdInfo.MethodInfo.GetParameters().ToParam();
            var parametesValues = CmdStringToParamsBindings.Inst.ResolveFromCmd(paramsStr, parameters, splitter: '/');
            return parametesValues.ToArray();
        }

        /// <summary>
        /// НЕ ПЕРЕДАВАТЬ НИКАКИХ ПАРАМЕТРОВ. 
        /// </summary>
        CmdInfoAttribute GetCurrentMemberAttribute(string memberName)
        {
            CmdInfoAttribute attr = GetType().GetMethod(memberName)
                .GetCustomAttribute(typeof(CmdInfoAttribute)) as CmdInfoAttribute;
            if (attr == null)
                throw new Exception("Can call this method only from methods with CmdInfoAttribute.");
            return attr;
        }

        [CmdInfo(CmdName = "q", Description = "Используйте эту команду для выхода из консоли.")]
        public void QuitCmd()
        {
            IsInRun = false;
        }

        /// <summary>
        /// Создает словарь с названиями команд cli и соответствующими им методы.
        /// </summary>
        Dictionary<string, ResolvedCmdInfo> CreateReflectionDict()
        {
            var cmdNameAndMethod = new Dictionary<string, ResolvedCmdInfo>();
            foreach (MethodInfo item in GetType().GetMethods())
            {
                CmdInfoAttribute attr = item.GetCustomAttribute(typeof(CmdInfoAttribute)) as CmdInfoAttribute;
                if (attr != null)
                {
                    var name = attr?.CmdName ?? TextExtensions.ToUnderscoreCase(item.Name);
                    var newCmdInfo = new ResolvedCmdInfo()
                    {
                        CmdName = name,
                        Description = attr.Description,
                        MethodInfo = item,
                        OriginalMethodName = item.Name
                    };
                    cmdNameAndMethod.Add(
                        name,
                        newCmdInfo)
                        ;
                }
            }
            return cmdNameAndMethod;
        }

        /// <summary>
        /// Надстройка для получения ресурсов через this с кешированием и по имени метода из консоли, а не только по имени ресурса.
        /// </summary>
        protected T ReadResource<T>(string resourceName, ReadResourseOptions options = null,
           [CallerMemberName]string memberName = null)
        {
            return (T)ReadResource(typeof(T), resourceName, options, memberName);
        }

        /// <summary>
        /// Надстройка для получения ресурсов через this с кешированием и по имени метода из консоли, а не только по имени ресурса.
        /// </summary>
        protected object ReadResource(Type resourceType, string resourceName, ReadResourseOptions options = null,
          [CallerMemberName]string memberName = null)
        {
            //Включаем автосчитывание если нужно
            options = options ?? new ReadResourseOptions();
            return Cmd.ReadResource(resourceType, memberName + "." + resourceName, options);
        }
    }
}
