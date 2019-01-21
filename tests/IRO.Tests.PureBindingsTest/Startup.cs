using System;
using IRO.Mvc.PureBinding;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace IRO.Tests.PureBindingsTest
{
    public class Startup
    {
        IServiceProvider _rootServiceProvider;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            JsonSerializerSettings jsonSerializerSettings = null;
            services.AddMvc()
                //.AddApplicationPart(typeof(OhlcController).Assembly)
                //.AddControllersAsServices()
                .AddJsonOptions(jsonOptions =>
                {
                    jsonSerializerSettings = jsonOptions.SerializerSettings;
                    jsonSerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
                    jsonSerializerSettings.NullValueHandling = NullValueHandling.Include;
                    //jsonSerializerSettings.TypeNameHandling = TypeNameHandling.All;
                    //jsonSerializerSettings.ContractResolver=new DefaultContractResolver();
                })
                .InsertJsonPureBinder(jsonSerializerSettings);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {           
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseMvc();

            _rootServiceProvider=app.ApplicationServices;
        }
    }
}
