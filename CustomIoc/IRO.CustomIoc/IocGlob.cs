using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IRO.CustomIoc
{
    public static class IocGlob
    {
        public static bool IsInit { get; private set; }

        public static void Init(IIocSystem ioc)
        {
            if (IsInit)
            {
                throw new Exception("IoC system was initialized before.");
            }
            _ioc = ioc ?? throw new Exception("Ioc system can`t be null.");
            IsInit = true;
        }

        static IIocSystem _ioc;
        public static IIocSystem Ioc
        {
            get
            {
                if (!IsInit)
                {
                    throw new Exception("IocGlob wasn`t initialized.");
                }
                return _ioc;
            }
        }
        
    }
}