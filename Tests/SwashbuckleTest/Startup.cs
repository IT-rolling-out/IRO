using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ItRollingOut.CoolSwagger;
using ItRollingOut.MvcExceptionHandler;
using ItRollingOut.MvcExceptionHandler.Models;
using ItRollingOut.MvcExceptionHandler.Services;
using ItRollingOut.MvcPart;
using ItRollingOut.PureBinding;
using ItRollingOut.PureBinding.SwaggerSupport;
using ItRollingOut.Reflection.CodeGen;
using ItRollingOut.Reflection.SummarySearch;
using ItRollingOut.Common.Diagnostics;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using SwashbuckleTest.Exceptions;
using SwashbuckleTest.Models;

namespace SwashbuckleTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .InsertJsonPureBinder();

            AddSwaggerGen_Local(services);

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
            UseExceptionBinder_Local(app, true);

            app.UseHttpsRedirection();
            app.UseMvc();

            
            app.Map("/error1", x => x.Run(ctx => throw new Exception()));
            app.Map("/error2", x => x.Run(ctx => throw new ClientException()));
            app.Map("/error3", x => x.Run(ctx => throw new NullReferenceException()));            
            app.Map("/error4", x => x.Run(async ctx =>
            {
                ctx.Response.StatusCode = 500;
            }));
            app.Map("/error5", x => x.Run(async ctx =>
            {
                ctx.Response.StatusCode = 401;
            }));
            app.Map("/error6", x => x.Run(async ctx =>
            {
                ctx.Response.StatusCode = 403;
            }));
            app.Map("/error7", x => x.Run(async ctx =>
            {
                //Тут исключение не возникнет, т.к. ContentLength > 0. 
                ctx.Response.StatusCode = 403;
                await ctx.Response.WriteAsync("q");
            }));
            app.Map("/error8", x => x.Run(async ctx =>
            {
                Task.Run(() =>
                {
                    throw new ClientException("Oh hi mark!");
                }).Wait();
            }));
            app.Map("/error9", x => x.Run(async ctx =>
            {
                ctx.Response.StatusCode = 400;
                var str=ctx.GetRequestBodyText();
            }));
            app.Map("/error10", x => x.Run(ctx => throw new NotFiniteNumberException()));
            app.Map("/error11", x => x.Run(async ctx =>
            {
                ctx.Response.StatusCode = 411;
            }));
            app.Map("/error12", x => x.Run(async ctx =>
            {
                ctx.Response.StatusCode = 422;
            }));

            UseSwaggerUI_Local(app);
        }

        void UseSwaggerUI_Local(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                //c.InjectJavascript("https://code.jquery.com/jquery-3.3.1.min.js");
                //c.InjectJavascript("/SwaggerClient/SwaggerInjectedJs/Script.js");
                c.ShowExtensions();
                c.EnableValidator();
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.DisplayOperationId();
                c.DisplayRequestDuration();
            });
        }

        void UseExceptionBinder_Local(IApplicationBuilder app, bool isDebug)
        {
            app.UseMvcExceptionHandler((s) =>
            {
                s.ErrorDescriptionUrlHandler = new FormattedErrorDescriptionUrlHandler("https://iro.com/errors/{0}");
                s.IsDebug = isDebug;
                s.DefaultHttpCode = 500;
                s.InnerExceptionsResolver = InnerExceptionsResolvers.InspectAggregateException;
                s.CanBindByHttpCode = true;
                s.JsonSerializerSettings.Formatting = Formatting.Indented;
                s.OwnExceptionsHandler += (ex) =>
                {
                    Debug.WriteLine("Exception in MvcExceptionHandler --> " + ex.ToString());
                };
                s.FilterAfterDTO = async (errorContext) =>
                {
                    if (errorContext.Configs.IsDebug && errorContext.ErrorInfo.ExceptionType!=null)
                    {
                        errorContext.ResponseDTO.AdditionalData = new Dictionary<string, object>();
                        if (typeof(ClientException).IsAssignableFrom(errorContext.ErrorInfo.ExceptionType))
                            errorContext.ResponseDTO.AdditionalData["WillBeUsedInProduction"] = true;
                        else
                            errorContext.ResponseDTO.AdditionalData["WillBeUsedInProduction"] = false;
                    }

                    //Can use middlevare services here
                    //configs.KeyGenerator.GenerateErrorKey(typeof(Exception));
                    return false;
                };

                //Кастомный обработчик ответа (без errorDTO).
                s.FilterBeforeDTO = async (errorContext) =>
                {
                    //Кастомная обработка исключений. Возвращаем ложь, т.к. ничего не сделал.
                    return false;
                };

                s.Mapping((builder) =>
                {
                    //Регистрируем исключение по http коду
                    builder.Register(
                        httpCode: 500,
                        errorKey: "InternalServerError"
                        );
                    builder.Register(
                        httpCode: 403,
                        errorKey: "Forbidden"
                        );
                    builder.Register(
                        httpCode: 401,
                        errorKey: "Unauthorized"
                        );
                    builder.Register(
                        httpCode: 400,
                        errorKey: "BadRequest"
                        );

                    //Регистрируем исключение, явно указывая ErrorKey.
                    builder.Register<ArgumentNullException>(
                        httpCode: 555,
                        errorKey: "CustomErrorKey"
                        );

                    //Регистрируем исключение автоматически указав ErrorKey - "NullReference" и используя http код по-умолчанию.
                    builder.Register<NullReferenceException>();

                    //Альтернативный способ регистрации.
                    builder.Register(new ErrorInfo()
                    {
                        ErrorKey = "MyError",
                        ExceptionType = typeof(NotImplementedException),
                        HttpCode = 556
                    });

                    //Регистрируем всех наследников этого исключения.
                    //Точнее, там "lazy registration", только по запросу самого исключения.
                    //builder.RegisterAllAssignable<Exception>(
                    //    httpCode: 411,
                    //    errorKeyPrefix: "BaseEx_"
                    //    );
                    builder.RegisterAllAssignable<ClientException>(
                        httpCode: 422,
                        errorKeyPrefix: "ClientEx_"
                        );                    
                });
            });
        }

        void AddSwaggerGen_Local(IServiceCollection services)
        {
            services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc(
                    "v1",
                    new Info
                    {
                        Title = "My API",
                        Version = "v1",
                        Description = "Api description"
                    });
                opt.SwaggerDocAdditional(doc =>
                {
                    doc.Schemes = new List<string>
                    {
                        "http",
                        "https"
                    };
                });
                opt.IncludeAllAvailableXmlComments();
                opt.UseCoolSummaryGen();
                opt.EnableAnnotations();
                opt.DescribeAllEnumsAsStrings();
                opt.UseReferencedDefinitionsForEnums();
                opt.UseDefaultIdentityAuthScheme();
                opt.AddPureBindingToSwashbuckle();
                opt.AddDefaultCoolSwaggerFilters();
            });
        }
    }
}
