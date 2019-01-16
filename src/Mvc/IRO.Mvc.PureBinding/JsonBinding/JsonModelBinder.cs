using IRO.Mvc;
using IRO.Mvc.PureBinding.Base;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
using IRO.Mvc.Core;

namespace IRO.Mvc.PureBinding.JsonBinding
{
    public class JsonModelBinder : BaseModelBinder
    {
        internal static JsonSerializer JsonSerializerProp { get; set; }

        private const string ParamNameForItemsBuff = "_parsedJsonToJToken";

        public JsonModelBinder(Type modelType, Func<IModelBinder> defaultModelBinderResolver) 
            : base(modelType, defaultModelBinderResolver)
        {
        }

        public override async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.Result.IsModelSet)
                return;
            try
            {
                if (CheckIfParameterIsMarked(bindingContext))
                {
                    await ResolveParam(bindingContext);
                }
                else
                {
                    //not our target
                }
            }
            catch (Exception ex)
            {
                var agrEx = new AggregateException(
                    $"Exception in model binder {nameof(JsonModelBinder)} while processing parameter '{ParamName ?? ""}'.",
                    ex
                    );

                //bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, agrEx, bindingContext.ModelMetadata);

                if (PureBindings.ThrowExceptions)
                {
                    throw agrEx;
                }
            }
        }

        private Task ResolveParam(ModelBindingContext bindingContext)
        {
            var req = bindingContext.HttpContext.Request;
            object res;
            if (req.Method == "POST")
            {
                //on post
                if (req.ContentType.Contains("json"))
                {
                    res = ResolveFromPostJsonBody(bindingContext);
                }
                else if (req.HasFormContentType)
                {
                    //Use default model binder for forms data.
                    throw new Exception($"Use FromPureBinding for form content now not available.");
                    //await DefaultModelBinder.BindModelAsync(bindingContext);
                    //return;
                }
                else
                {
                    throw new Exception($"Unsapported http post content-type.");
                }
            }
            else
            {
                throw new Exception($"Unsapported request method. Supported only GET and POST.");
            }

            bindingContext.Result = ModelBindingResult.Success(res);

            return Task.CompletedTask;
        }

        private object ResolveFromPostJsonBody(ModelBindingContext bindingContext)
        {
            //values from all body as json            
            var httpCtx = bindingContext.HttpContext;
            var req = bindingContext.HttpContext.Request;

            JToken jToken;
            if (httpCtx.Items.ContainsKey(ParamNameForItemsBuff))
            {
                //if json body was parsed for previous parameters.
                jToken = (JToken)httpCtx.Items[ParamNameForItemsBuff];
            }
            else
            {
                string bodyStr = httpCtx.GetRequestBodyText();
                using (JsonReader reader = new JsonTextReader(new StringReader(bodyStr)))
                {
                    jToken = JsonSerializerProp.Deserialize<JToken>(reader);
                    httpCtx.Items[ParamNameForItemsBuff] = jToken;
                }
            }

            var resultValue = jToken[ParamName].ToObject(ParameterType);
            return resultValue;
        }
    }
}
