using System.Reflection;

namespace IRO.Reflection.Core
{
    public class Parameter
    {
        /// <summary>
        /// Может отличаться от имени из ParameterInfo.
        /// </summary>
        public string ParamName { get; set; }

        public ParameterInfo Info { get; set; }
    }
}
