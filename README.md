# IRO Tools main repository

In current repository you can see sources of my nuget packages, which I use as a foundation for my more specific libs and projects.

All available on [nuget.org](nuget.org).

Else projects based on it:
- [IRO.Mvc.MvcExceptionHandler](https://github.com/IT-rolling-out/IRO.Mvc/tree/master/src/Mvc/IRO.Mvc.MvcExceptionHandler) - flexible exception handler for ASP.NET Core.
- [IRO.Mvc.CoolSwagger](https://github.com/IT-rolling-out/IRO.Mvc/tree/master/src/Mvc/IRO.Mvc.CoolSwagger) - Swashbuckle extensions.
- [IRO.Mvc.PureBinding](https://github.com/IT-rolling-out/IRO.Mvc/tree/master/src/Mvc/IRO.Mvc.PureBinding) - model binder for controller methods.

### Contributing

1. [How to build?](docs/Contributing/Build.md)
1. [Solution architecture and naming style.](docs/Contributing/Sln.md)
1. [What is Resharper.Dotnet?](docs/Contributing/Resharper.md)

# Below is a description and examples of projects in this repository.

### IRO.LoggingExt

This can help you log information about method calls and exceptions. And it works without recompilation like in PostSharp.

Example:

having this method
```csharp
        int Sum(int a, int b)
        {
            var sum = a + b;
			var someInnerVariable="aaaa";
            return sum;
        }
```

if not use extensions to log all information about method we can use this code
```csharp
        int Sum(int a, int b)
        {
            try
            {
                var sum = a + b;				
			    var someInnerVariable="aaaa";

                var msg = "Method {CallerNamespase}.{CallerClass}.{CalledMethod} called." +
                          "\n\nWith arguments: {Argument_a}, {Argument_b}." +
                          "\n\nWith additional values: {someInnerVariable}." +
                          "\n\nReturned: {Result}";
                _logger.LogInformation(
                    msg,
                    GetType().Namespace,
                    GetType().Name,
                    nameof(Sum),
                    a,
                    b,
                    someInnerVariable,
                    sum
                );
                return sum;
            }
            catch (Exception ex)
            {
                var msg = "Method {CallerNamespase}.{CallerClass}.{CalledMethod} called." +
                          "\n\nWith arguments: {Argument_a}, {Argument_b}." +
                          "\n\nFinished with exception: {Exception}";
                _logger.LogInformation(
                    msg,
                    GetType().Namespace,
                    GetType().Name,
                    nameof(TestNormal_Analog),
                    a,
                    b,
                    ex
                );
                throw;
            }
        }
```

with IRO.LoggingExt you can write logging like this and it will write same message
```csharp
        async Task<int> AsyncTestNormal(int a, int b)
        {
            using var methodLogScope = _loggingExt
                .MethodLogScope(this)
                .WithArguments(a, b, 100)
               

            var sum = a + b;
            var someInnerVariable="aaaa";
			methodLogScope.WithAdditionalValue("someInnerVariable", someInnerVariable);
			
            return methodLogScope.WithReturn(sum);
        }
```


### IRO.Storage

Simple key-value storage for client applications.

```csharp
    await storage.Set("key", 100);
    var num = (int)await storage.Get(typeof(int), "key");
    num = await storage.Get<int>("key");
    int? nullableNum = await storage.GetOrDefault<int?>("key");
    
	//Remove
	await storage.Remove("key");
    bool isContains = await storage.Contains("key"); //False

    //Null values
    await storage.Set("key", null);
    isContains = await storage.Contains("key"); //True

    //Scope usage (use dot symbol for this)
    await storage.Set("Scope.Value1", 10);
    await storage.Set("Scope.Value2", "Str");
    var scope = await storage.Get<MyScopeClass>("Scope");
    //Or this
    //JToken scope = await storage.Get("Scope");
    Console.WriteLine(scope.Value1); //Write 10
    Console.WriteLine(scope.Value2); //Write Str

    //Scope remove
    await storage.Remove("Scope");     
    //Note that if set value to this key - scope will be removed automatically.
    isContains = await storage.Contains("Scope"); //False
    isContains = await storage.Contains("Scope.Value1"); //False too
```

Implemented storages:
- json file;
- ram storage;
- based on litedb.

Has default caching and cross-process synchronization.

### IRO.Threading

Now contains only ThreadSyncContext which can help you working with specific threads.
Wpf example. How you can get value of wpf ui contol (with passing exceptions to calling thread):

```csharp
    Visibility value = default(Visibility);
    Exception internalException = null;
    control.Dispatcher.Invoke(()=>
    {
        try
        {
            value = control.Visibility;
        }
        catch(Exception ex)
        {
            internalException = ex;
        }
    });
    if(internalException!=null)
        throw internalException;
    return value;
```

How you can do it with ThreadSyncContext:

```csharp
    return ThreadSyncContext.Invoke(()=> control.Visibility);
```

### IRO.EmbeddedResources

Helps to read or extract embedded resource files and directories.

### IRO.Cache

Has cache service interface and simplest implemention (based on records count). Good solution for client apps.

### IRO.FileIO.ImprovedFileOperations

Class ImprovedFile has some operations same to File/Directory classes, but recursive and with logging. 
Used in my own project, but can be useful for you.

### IRO.Reflection.Core

Add some base reflection operations. What you can do:
- get type name just same as c# code (default generic names is not same);
- build Type inheritance tree (and convert it to sorted list;
- invoke any method with json array or complex object. Useful to build cross-process interfaces;
- find assignable types;
- find types with attribute4
- create generic list or dictionary with reflection;
- create types recursive.

### IRO.Reflection.SummarySearch

Work with members xml comments through reflection classes like Type, MethodInfo, PropertyInfo etc.

```csharp
    var xml = DocsParser.XmlFromMethod(context.MethodInfo);
    string summaryText = DocsParserExtensions.XmlSummaryToString(xml);
```

NOTE: Add following code to your csproj PropertyGroup to enable generation of summary files.

```xml
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
	<NoWarn>$(NoWarn);1591</NoWarn>
```

### IRO.Reflection.CodeGen

Most important class of current assembly is SourceFileContext.

Current class can allow you opportunity to simple manage dependencies of cs files and than use it to generate using section and compile your code in runtime.

```csharp
            const string Source = @"
//Use types from different namespaces and assemblies.
public class GenClass1
{
    public int Num { get; set; }=100;
    
    public Stream SomeProp1 { get; set; }

    public List<string> SomeProp2 { get; set; }

    public Task<Dictionary<string, MemberInfo>> SomeMethod1(){ return null; }
}
";
            //Easy manage depemdensies.
            var ctx = new SourceFileContext();
            ctx.UsedType(typeof(int));
            ctx.UsedType(typeof(Stream));
            ctx.UsedType(typeof(List<string>));

            //Works with generic parameters automatically.
            ctx.UsedType(typeof(Task<Dictionary<string, MemberInfo>>));

            //Add namespace and usings.
            var source = CodeGenExtensions.WrapClass(
                Source,
                "MyNamespace",
                ctx.GetNamespaces()
                );
            var compilerInputData = new CompilerInputData()
            {
                CSharpCode = source,
                ReferencedAssemblies = ctx.GetAssemblies()
            };
            var assembly = Compiler.Compile(compilerInputData);
            var type = assembly.GetType("MyNamespace.GenClass1");
            dynamic modelInstance = Activator.CreateInstance(type);
            int num = modelInstance.Num;
```