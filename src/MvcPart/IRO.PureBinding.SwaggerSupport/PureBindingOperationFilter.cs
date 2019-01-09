﻿using IRO.PureBinding.Metadata;
using IRO.Reflection.CodeGen;
using IRO.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;

namespace IRO.PureBinding.SwaggerSupport
{
    public class PureBindingOperationFilter : IOperationFilter
    {       
        public void Apply(Operation operation, OperationFilterContext context)
        {
            ISchemaRegistry _schemaRegistry = context.SchemaRegistry;
            
            bool usedPureBinding = GenerateModelIfPureBinding(context.MethodInfo, out var generatedType);
            if (usedPureBinding)
            {
                var scheme = _schemaRegistry.GetOrRegister(generatedType);
                operation.Parameters.Clear();
                operation.Parameters.Add(new BodyParameter()
                {
                    Schema = scheme
                });
            }
        }

        bool GenerateModelIfPureBinding(MethodInfo methodInfo, out Type generatedType)
        {
            if (methodInfo.GetCustomAttribute<HttpGetAttribute>() != null)
            {
                generatedType = null;
                return false;
            }

            string namespaceStr = "GeneratedSwaggerModels_PureBinding";
            string modelName = methodInfo.DeclaringType.Name.Replace("Controller", "");
            modelName += "_" + methodInfo.Name;// + TextExtensions.Generate(5);

            var modelGenInp = new ModelsGeneratorInput()
            {
                Params = new List<IRO.Reflection.Core.Parameter>(),
                ModelName=modelName
            };
            foreach (var param in methodInfo.GetParameters())
            {
                var attr=param.GetCustomAttribute<FromPureBindingAttribute>();
                if (attr == null)
                    continue;
                var customParam = new IRO.Reflection.Core.Parameter();
                customParam.ParamName=attr.ParameterName ?? param.Name;
                customParam.Info = param;
                modelGenInp.Params.Add(customParam);
            }

            if (!modelGenInp.Params.Any())
            {
                generatedType = null;
                return false;
            }

            var modelGen = new ModelsGenerator(
                new ModelsGeneratorInput[] { modelGenInp }, 
                namespaceStr,
                typeNamesWithAssembly:true
                );
            var codeGenResult = modelGen.Generate();
            var asm=codeGenResult.Compile();
            generatedType=asm.GetType(namespaceStr + "." + modelName);
            return true;
        }
    }


}
