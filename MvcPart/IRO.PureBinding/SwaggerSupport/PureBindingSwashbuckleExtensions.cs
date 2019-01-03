using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace IRO.PureBinding.SwaggerSupport
{
    public static class PureBindingSwashbuckleExtensions
    {
        public static void AddPureBindingToSwashbuckle(this SwaggerGenOptions opt)
        {
            opt.OperationFilter<PureBindingOperationFilter>();
        }
    }


}
