using IRO.Mvc.PureBinding.Base;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace IRO.Mvc.PureBinding.JsonBinding
{
    public class JsonModelBinderProvider : BaseModelBinderProvider
    {
        public override IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            base.GetBinder(context);

            if (!PureBindings.JsonIsInit)
            {
                throw new Exception($"Json pure binder wasn`t initialized.");
            }

            if (CheckIfUseOurBinder(context))
            {
                return new JsonModelBinder(
                    context.Metadata.ModelType, 
                    ()=> CreateDefaultBinder(context)
                    );
            }
            return null;
        }
        
    }

}
