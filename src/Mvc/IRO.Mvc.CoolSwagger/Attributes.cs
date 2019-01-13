using System;
using System.Collections.Generic;
using System.Text;

namespace IRO.Mvc.CoolSwagger
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SwaggerTagNameAttribute : Attribute
    {
        public string TagName { get; }

        public SwaggerTagNameAttribute(string tagName)
        {

            TagName = tagName;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SwaggerIgnoreAttribute : Attribute
    {
        public SwaggerIgnoreAttribute()
        {
        }
    }
}
