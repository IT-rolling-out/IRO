using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IRO.Mvc.CoolSwagger
{
    /// <summary>
    /// Наход методы, которые требуют авторизации и записывает это в swagger.json .
    /// </summary>
    public class IdentityAuthOperationFilter: IOperationFilter
    {
        string _securityDefinitionName;
        IEnumerable<string> _scopes;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scopes">Достаточно одной строки, не важно что там будет.</param>
        public IdentityAuthOperationFilter(IEnumerable<string> scopes, string securityDefinitionName)
        {
            //if (scopes == null || !scopes.Any())
            //{
            //    throw new Exception("Swagger scopes array must contains one or more strings.");
            //}
            _scopes = scopes;
            _securityDefinitionName = securityDefinitionName;
        }

        public void Apply(Operation operation, OperationFilterContext context)
        {
            try
            {
                if (!CheckNeedIdentityAuth(context.MethodInfo))
                    return;

                operation.Responses.Add("401", new Response { Description = "Unauthorized" });
                operation.Responses.Add("403", new Response { Description = "Forbidden" });

                operation.Security = new List<IDictionary<string, IEnumerable<string>>>
                {
                    new Dictionary<string, IEnumerable<string>>
                    {
                        { _securityDefinitionName, _scopes }
                    }
                };
            }
            catch { }          
        }       

        bool CheckNeedIdentityAuth(MethodInfo methodInfo)
        {
            var controllerType = methodInfo.DeclaringType;
            if (methodInfo.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any())
            {
                return true;
            }

            if (methodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any())
                return false;

            bool controllerNeedAuth = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();
            return controllerNeedAuth;
        }
    }
}
