using System;
using System.Reflection;

namespace IRO.Reflection.Core.ModelBinders
{
    public struct Parameter
    {
        public string ParamName { get; set; }

        public Type ParamType { get; set; } 

        /// <summary>
        /// Can be null if object created manually.
        /// </summary>
        public ParameterInfo ParamInfo { get; set; } 
    }
}
