using IRO.Mvc.PureBinding.Base;
using Microsoft.AspNetCore.Mvc;
using System;

namespace IRO.Mvc.PureBinding.Metadata
{
    /// <inheritdoc />
    /// <summary>
    /// Search first modelbinder that implementing type BasePureBindingModelBinder.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromPureBindingAttribute : ModelBinderAttribute
    {
        //You can set parameter name here or will be used default name (name of parameter in c#).
        public string ParameterName { get; set; }

        public FromPureBindingAttribute() : base(typeof(EmptyModelBinder))
        {

        }
    }
}
