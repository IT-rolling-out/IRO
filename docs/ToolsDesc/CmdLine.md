# IRO.CmdLine

Сборка предоставляет мощный инструмент для постоения консольных приложений.
CmdLine позволяет сгенерировать cli из методов класса-наследника `CommandLineBase`. Создайте такой класс и пропишите в нем методы, которые должны быть доступны в консоли, отметьте их атрибутом `[CmdInfo]`.
В примере этот класс назван `CmdLineFacade`. 
В Main() инициализируйте консоль. И да, нужно обязательно инициализировать хранилище.
```csharp
            StorageHardDrive.InitDependencies(
               new JsonLocalStorage()
               );

            //Простейшая консоль с командами из методов классса.
            CmdLineExtension.Init(new DefaultConsoleHandler());
            var cmds = new CmdSwitcher();
            cmds.PushCmdInStack(new CmdLineFacade());
            cmds.ExecuteStartup(args);
            if (args.Length == 0) 
                cmds.RunDefault();
            Console.ReadLine();
```
Запустите и пропишите команду `help` чтоб посмотреть доступные методы.
В самом CmdLineFacade рекомендуется использовать protected свойство Cmd.
В нем есть все для ввода/вывода не только строк, но и комплексных объектов, что очень ускоряет работу.
Особое внимание обратите на метод `Cmd.ReadResource`.
Также вы можете передать параметры в метод при вызове команды. Пример:
`copy /path:"E\\qwe" /overwrite:true`
Таким образом вы можете вызвать команду передав параметры запуска приложения. Это очень удобно для написания утилит командной строки.
Пример реализации такого - FilesReplacerUtil.