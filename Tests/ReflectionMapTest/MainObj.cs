using ItRollingOut.Reflection.Map.Metadata;
using System.Threading.Tasks;

namespace ReflectionMapTest
{
    class MainObj
    {
        [SimplePropReflectionMap]
        public string AccessToken { get; }

        [SimplePropReflectionMap]
        public bool IsTokenAttached { get; }

        [IncludedObjReflectionMap]
        public SubObj CurrentSubObj { get; } = new SubObj();

        /// <summary>
        /// Выполняет вход в аккаунт, задает токен доступа. Учтите, что не все методы требуют входа.
        /// </summary>
        [MethodReflectionMap]
        public async Task<string> MyMethod(string str)
        {
            return str + "!!!";
        }

    }
}
