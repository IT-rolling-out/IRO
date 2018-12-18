using ItRollingOut.CustomIoc;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace IocTests
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
