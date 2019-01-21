using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using IRO.FileIO.ImprovedFileOperations;

namespace IRO.Mvc.CoolSwagger
{
    public static class CoolSwaggerExtensions
    {
        public static void AddDefaultCoolSwaggerFilters(this SwaggerGenOptions opt)
        {
            opt.OperationFilter<SwaggerTagNameOperationFilter>();
        }

        public static void UseCoolSummaryGen(this SwaggerGenOptions opt)
        {
            opt.OperationFilter<SummaryOperationFilter>();
        }

        public static void IncludeXmlCommentsOfAssembly(this SwaggerGenOptions opt, Assembly assembly)
        {
            var path = Path.ChangeExtension(assembly.CodeBase, ".xml");
            if (!File.Exists(path))
            {
                throw new Exception($"Can`t find comments file '{path}'.");
            }
            opt.IncludeXmlComments(path);
        }

        public static void IncludeXmlCommentsOfAssembly(this SwaggerGenOptions opt, Type typeFromAssembly)
        {
            IncludeXmlCommentsOfAssembly(opt, typeFromAssembly.Assembly);
        }

        public static void IncludeAllAvailableXmlComments(this SwaggerGenOptions opt)
        {
            var xmlFiles=(new ImprovedFile()).Search(
                AppContext.BaseDirectory,
                new List<Regex> { new Regex("\\.[Xx][Mm][Ll]$") }
                );
            //var xmlFilesOfAssembly = new List<string>();
            foreach (var xmlFile in xmlFiles)
            {
                var assemblyFileDll = Path.ChangeExtension(xmlFile, ".dll");
                var assemblyFileExe = Path.ChangeExtension(xmlFile, ".exe");
                if (File.Exists(assemblyFileDll) || File.Exists(assemblyFileExe))
                {
                    //xmlFilesOfAssembly.Add(xmlFile);
                    opt.IncludeXmlComments(xmlFile); 
                }
            }
        }

        public static void UseDefaultIdentityAuthScheme(this SwaggerGenOptions opt)
        {
            opt.OperationFilter<IdentityAuthOperationFilter>(new string[] { }, "Bearer");

            //var security = new Dictionary<string, IEnumerable<string>>
            //{
            //    {"Bearer", new string[] { }},
            //};
            opt.AddSecurityDefinition("Bearer", new ApiKeyScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\".",
                Name = "Authorization",
                In = "header",
                Type = "apiKey"
            });
            //opt.AddSecurityRequirement(security);
        }

        public static void SwaggerDocAdditional(this SwaggerGenOptions opt, Action<SwaggerDocument> action)
        {
            opt.DocumentFilter<AdditionalSettingsDocumentFilter>(action);
        }
    }
}
