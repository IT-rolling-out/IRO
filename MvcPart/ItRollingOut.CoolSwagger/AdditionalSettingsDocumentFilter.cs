﻿using System;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ItRollingOut.CoolSwagger
{
    public class AdditionalSettingsDocumentFilter : IDocumentFilter
    {
        Action<SwaggerDocument> _action;

        public AdditionalSettingsDocumentFilter(Action<SwaggerDocument> action)
        {
            _action = action;
        }

        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            _action(swaggerDoc);
        }
    }
}