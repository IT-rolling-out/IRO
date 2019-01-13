using System;
using IRO.Reflection.Core;

namespace IRO.Mvc.MvcExceptionHandler.Services
{
    public class ErrorKeyGenerator : IErrorKeyGenerator
    {
        public string GenerateErrorKey(Type exceptionType)
        {
            //exceptionType = typeof(IDictionary<int, string>);
            if (exceptionType == typeof(Exception))
            {
                return "Exception";
            }
            else if (exceptionType.IsGenericType)
            {
                var normalTypeName = ReflectionHelpers.GetNormalTypeName(
                    exceptionType,
                    withNamespace: false
                    );
                normalTypeName = normalTypeName
                    .Replace(" ", "")
                    .Replace("\n", "")
                    .Replace("\t", "")
                    .Replace("\r", "");

                normalTypeName = normalTypeName
                    .Replace("<", "_0")
                    .Replace(">", "0_")
                    .Replace(",", "_");
                return normalTypeName;
            }
            else
            {
                //const string exTypeNameConst = "Exception";
                //var errorKey = exceptionType.Name;
                //if (exceptionType.Name.EndsWith(exTypeNameConst))
                //{
                //    errorKey = errorKey.Remove(errorKey.Length - exTypeNameConst.Length);
                //}
                return exceptionType.Name;
            }

        }

    }
}
