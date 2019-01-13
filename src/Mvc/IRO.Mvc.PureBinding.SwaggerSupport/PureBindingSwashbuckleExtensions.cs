using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc.Core;

namespace IRO.Mvc.PureBinding.SwaggerSupport
{
    public static class PureBindingSwashbuckleExtensions
    {
        public static void AddPureBindingToSwashbuckle(this SwaggerGenOptions opt)
        {
            opt.OperationFilter<PureBindingOperationFilter>();
        }
    }


}
