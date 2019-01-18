using System.Diagnostics;
using IRO.CustomIoc;

namespace IRO.SlnTests.IocTests
{
    public class IocTest
    {
        public IocTest(IIocSystem iocSystem)
        {
            Debug.WriteLine($"IIocSystem --> {iocSystem != null}");
            Debug.WriteLine($"IocGlob.Ioc --> {IocGlob.Ioc != null}");
            //Debug.WriteLine($"IServiceProvider --> {serviceProvider != null}");            
        }

        public static void Run()
        {
            var ioc = new IocSystem();    
            ioc.RegisterTransient<IocTest>();
            ioc.Build();
            ioc.Resolve<IocTest>();
        }
    }
}