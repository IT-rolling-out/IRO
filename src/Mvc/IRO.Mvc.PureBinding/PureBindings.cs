using IRO.Mvc.PureBinding.JsonBinding;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace IRO.Mvc.PureBinding
{
    public static class PureBindings
    {
        internal static bool JsonIsInit { get; private set; }

        /// <summary>
        /// Only for debug.
        /// </summary>
        public static bool ThrowExceptions { get; set; } = false;

        /// <summary>
        /// Use settings that you pass in parameters.
        /// Insert binder as first element in 'options.ModelBinderProviders' list.
        /// </summary>
        public static IMvcBuilder InsertJsonPureBinder(this IMvcBuilder builder, JsonSerializerSettings jsonSettings = null)
        {
            if (JsonIsInit)
                throw new System.Exception("Pure api json binder was initialized before.");
            if (jsonSettings == null)
            {
                builder.AddJsonOptions(jsonOpts =>
                {
                    jsonSettings = jsonOpts.SerializerSettings;
                });
            }
            JsonModelBinder.JsonSerializerProp = JsonSerializer.Create(jsonSettings);
            builder.AddMvcOptions(options =>
            {
                options.ModelBinderProviders.Insert(0, new JsonModelBinderProvider());
            });
            JsonIsInit = true;
            return builder;
        }
    }
}
