using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.CompilerServices;

namespace IRO.CustomIoc
{
    /// <summary>
    /// Смысл IocSystem в том чтоб накладывать дополнительные проверки на ServiceProvider и ServiceCollection,
    /// когда они используются в одном месте. 
    /// <para></para>
    /// Например, вы можете передать в какой-то плагин ссылку на IocSystem, регистрировать через нее сервисы, 
    /// а потом ресолвить их. IocSystem гарантирует, что вы будете использовать IServiceProvider конкретно этой IServiceCollection,
    /// при этом абстрагируя от процесса сборки этой самой коллекции (такое особо важно для IServiceProvider в asp.net).
    /// <para></para>
    /// При этом синхронизируется работа с Register и Resolve методами.
    /// Первый вызов Resolve билдит провайдер сервисов. После билда Register методы будут выкидывать исключение. 
    /// </summary>
    public class IocSystem : IIocSystem
    {
        bool disposedValue = false;
        Func<IServiceCollection, IServiceProvider> _builder;
        bool _firstBuildTry = true;

        public event IocBuildedDelegate Builded;

        /// <summary>
        /// Если истина, то при любом вызове ServiceProvider или нуждающегося в нем метода будет автоматически вызван
        /// метод Build(). Иначе - выкинуто исключение. 
        /// По-умолчанию false.
        /// </summary>
        public bool Autobuild { get; set; }

        IServiceProvider _serviceProvider;       
        public IServiceProvider ServiceProvider
        {
            get
            {
                ThrowIfDisposed();
                ThrowIfNotBuilded();                
                return _serviceProvider;
            }
        }

        IServiceCollection _serviceCollection;       
        public IServiceCollection ServiceCollection
        {
            get
            {
                ThrowIfDisposed();
                ThrowIfBuilded();
                return _serviceCollection;
            }
        }

        public bool IsBuilded { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="builder">Default will just call serviceCollection.BuildServiceProvider();</param>
        public IocSystem(IServiceCollection serviceCollection, Func<IServiceCollection, IServiceProvider> builder = null)
        {
            if (builder == null)
            {
                //Standart builder.
                _builder = (servCollection) => servCollection.BuildServiceProvider();
            }
            else
            {
                _builder = builder;
            }
            _serviceCollection = serviceCollection;
        }

        /// <summary>
        /// Использует стандартную ServiceCollection.
        /// </summary>
        public IocSystem():this(new ServiceCollection())
        {
        }

        public void RegisterSingletone(Type type, object inst)
        {
            ServiceCollection.AddSingleton(type, inst);
        }

        public void RegisterSingletone(Type type)
        {
            ServiceCollection.AddSingleton(type);
        }

        public void RegisterTransient(Type baseType, Type inheritType)
        {
            ServiceCollection.AddTransient(baseType, inheritType);
        }

        public void RegisterScoped(Type baseType, Type inheritType)
        {
            ServiceCollection.AddScoped(baseType, inheritType);
        }

        public object Resolve(Type baseType)
        {
            return ServiceProvider.GetRequiredService(baseType);
        }

        public void Dispose()
        {
            if (!disposedValue)
            {
                _serviceCollection = null;
                _serviceProvider = null;
                disposedValue = true;
            }
        }

        public void Build()
        {
            ThrowIfDisposed();
            ThrowIfBuilded();

            if (_firstBuildTry)
            {
                //Регистрирует свой инстанс.
                _serviceCollection.AddSingleton<IIocSystem>(this);
                _firstBuildTry = false;
            }

            _serviceProvider = _builder(_serviceCollection);
            if (_serviceProvider == null)
                throw new Exception("IocSystem build failed. Builded ServiceProvider can`t be null.");
            _serviceCollection = null;
            IsBuilded = true;
            Builded?.Invoke(this);
        }

        public IServiceScope CreateScope()
        {
            return ServiceProvider.CreateScope();
        }

        void ThrowIfDisposed([CallerMemberName]string memberName="MemberName")
        {
            if (disposedValue)
                throw new ObjectDisposedException($"You can`t use '{memberName}' in disposed IocSystem.");
        }

        void ThrowIfBuilded([CallerMemberName]string memberName = "MemberName")
        {
            if (IsBuilded)
                throw new Exception($"You can`t use '{memberName}' after IocSystem build (or first resolve call).");
        }

        void ThrowIfNotBuilded([CallerMemberName]string memberName = "MemberName")
        {
            if (!IsBuilded && Autobuild)
            {
                Build();
            }
            if (!IsBuilded)
                throw new Exception($"You can`t use '{memberName}' before IocSystem is builded. You can solve it if enable Autobuild.");
        }
    }
}