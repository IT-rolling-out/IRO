using IRO.Mvc.PureBinding.JsonBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace IRO.Mvc.PureBinding.Base
{
    public class BaseModelBinderProvider:IModelBinderProvider
    {
        public virtual IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }            
            return null;
        }

        protected bool CheckIfUseOurBinder(ModelBinderProviderContext context)
        {
            return context.BindingInfo.BinderType?.IsAssignableFrom(typeof(EmptyModelBinder)) == true;
        }

        /// <summary>
        /// Creates default ModelBinder
        /// <para></para>
        /// Can return null
        /// </summary>
        protected IModelBinder CreateDefaultBinder(ModelBinderProviderContext context)
        {
            var defaultBindersProvider = context.Services.GetRequiredService<IOptions<MvcOptions>>().Value.ModelBinderProviders;

            return defaultBindersProvider
                .Where(binderPr => !(binderPr is JsonModelBinderProvider))
                .Select(binder => binder.GetBinder(context))
                .FirstOrDefault(binder => binder != null);
        }
    }
}
