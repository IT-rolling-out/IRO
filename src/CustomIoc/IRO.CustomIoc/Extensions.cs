using System;

namespace IRO.CustomIoc
{
    public static class Extensions 
    {
        public static void RegisterSingletone<TService>(this IIocSystem iocSystem, TService inst)
            where TService : class
        {
            iocSystem.RegisterSingletone(typeof(TService), inst);
        }

        public static void RegisterSingletone<TService>(this IIocSystem iocSystem)
            where TService : class
        {
            iocSystem.RegisterSingletone(typeof(TService));
        }

        public static void RegisterTransient<TImplemention, TBase>(this IIocSystem iocSystem)
            where TImplemention : TBase
        {
            iocSystem.RegisterTransient(typeof(TBase), typeof(TImplemention));
        }

        /// <summary>
        /// Простейший метод регистрации типа. Поддерживается любой ioc и даже FactoryBox.
        /// Регистрирует тип как самого себя.
        /// </summary>
        public static void RegisterTransient<TService>(this IIocSystem iocSystem)
        {
            iocSystem.RegisterTransient(typeof(TService), typeof(TService));
        }

        /// <summary>
        /// Простейший метод регистрации типа. Поддерживается любой ioc и даже FactoryBox.
        /// Регистрирует тип как самого себя.
        /// </summary>
        public static void RegisterTransient(this IIocSystem iocSystem, Type serviceType)
        {
            iocSystem.RegisterTransient(serviceType, serviceType);
        }

        public static void RegisterScoped<TImplemention, TBase>(this IIocSystem iocSystem)
            where TImplemention : TBase
        {
            iocSystem.RegisterScoped(typeof(TBase), typeof(TImplemention));
        }

        /// <summary>
        /// Простейший метод регистрации типа. Поддерживается любой ioc и даже FactoryBox.
        /// Регистрирует тип как самого себя.
        /// </summary>
        public static void RegisterScoped<TService>(this IIocSystem iocSystem)
        {
            iocSystem.RegisterScoped(typeof(TService), typeof(TService));
        }

        /// <summary>
        /// Простейший метод регистрации типа. Поддерживается любой ioc и даже FactoryBox.
        /// Регистрирует тип как самого себя.
        /// </summary>
        public static void RegisterScoped(this IIocSystem iocSystem, Type serviceType)
        {
            iocSystem.RegisterScoped(serviceType, serviceType);
        }

        public static bool CanResolve(this IIocSystem iocSystem, Type type)
        {

            if (type.IsValueType)
                throw new Exception("'CanResolve' can`t work with value types.");
            try
            {
                var val = iocSystem.Resolve(type);
                return val != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool CanResolve<TService>(this IIocSystem iocSystem)
            where TService:class
        {
            return CanResolve(iocSystem, typeof(TService));
        }

        public static TService Resolve<TService>(this IIocSystem iocSystem)
        {
            return (TService)iocSystem.Resolve(typeof(TService));
        }

        public static void UsingScope(this IIocSystem iocSystem, Action<IServiceProvider> action)
        {
            using(var scope = iocSystem.CreateScope())
            {
                action(scope.ServiceProvider);
            }
        }
    }



}