using IRO.Mvc.PureBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IRO.Mvc.PureBinding.Base
{
    public abstract class BaseModelBinder : IModelBinder
    {
        protected Type ParameterType { get; }

        /// <summary>
        /// Name of parmeter from controller method.
        /// </summary>
        protected string ParamName { get; private set; }

        /// <summary>
        /// Name of parmeter, that contains all object with values for PureBinding.
        /// </summary>
        protected string NameOfPureBindingContainerParameter { get; private set; } = "r";

        IModelBinder _defaultModelBinder;
        protected IModelBinder DefaultModelBinder
        {
            get
            {
                if (_defaultModelBinder == null)
                    _defaultModelBinder = _defaultModelBinderResolver();
                return _defaultModelBinder;
            }
        }

        private bool? useOnIt;               

        Func<IModelBinder> _defaultModelBinderResolver;

        public BaseModelBinder(Type modelType, Func<IModelBinder> defaultModelBinderResolver) 
        {
            defaultModelBinderResolver = defaultModelBinderResolver ??
                throw new ArgumentNullException(nameof(defaultModelBinderResolver));
            modelType = modelType ??
                throw new ArgumentNullException(nameof(modelType));
            ParameterType = modelType;
            _defaultModelBinderResolver = defaultModelBinderResolver;
        }

        protected bool CheckIfParameterIsMarked(ModelBindingContext bindingContext)
        {
            //If first time
            if (useOnIt == null)
            {
                //if from controller
                if (bindingContext.ActionContext.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
                {

                    var currentParamInfo = actionDescriptor.MethodInfo.GetParameters()
                        .First(par => par.Name == bindingContext.FieldName);
                    var attrParam = currentParamInfo.GetCustomAttribute<FromPureBindingAttribute>();


                    if (attrParam == null)
                    {
                        useOnIt = false;
                    }
                    else
                    {
                        //save metadata here
                        ParamName = attrParam.ParameterName ?? bindingContext.FieldName;
                        var attrMethod = actionDescriptor.MethodInfo.GetCustomAttribute<PureBindingMethodSettingsAttribute>();
                        if (attrMethod != null)
                        {
                            NameOfPureBindingContainerParameter = attrMethod.NameGlobalHttpParameter;
                        }
                        useOnIt = true;
                    }
                }
                else
                {
                    throw new Exception($"Current model binder can work only with controllers, because it require attributes.");
                }

            }

            return useOnIt.Value;
        }

        public abstract Task BindModelAsync(ModelBindingContext bindingContext);
    }
}
