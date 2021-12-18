using Microsoft.Extensions.DependencyInjection;

namespace IRO.LoggingExt
{
    public static class ServiceCollectionInitExtensions
    {
        public static IServiceCollection AddMethodLogging(this IServiceCollection services)
        {
            services.AddSingleton<IMethodLoggerFactory>((sp) =>
            {
                return new MethodLoggerFactory(sp);
            });
            return services;
        }
    }
}