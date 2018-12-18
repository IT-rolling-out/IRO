using System;
using Microsoft.Extensions.DependencyInjection;

namespace ItRollingOut.CustomIoc
{   
    public interface IIocSystem
    {
        bool Autobuild { get; set; }
        bool IsBuilded { get; }
        IServiceCollection ServiceCollection { get; }
        IServiceProvider ServiceProvider { get; }

        event IocBuildedDelegate Builded;

        void Build();
        IServiceScope CreateScope();
        void Dispose();
        void RegisterScoped(Type baseType, Type inheritType);
        void RegisterSingletone(Type type);
        void RegisterSingletone(Type type, object inst);
        void RegisterTransient(Type baseType, Type inheritType);
        object Resolve(Type baseType);
    }

    public delegate void IocBuildedDelegate(IIocSystem sender);
}