using IRO.Mvc;
using IRO.Mvc.Core;
using Microsoft.AspNetCore.Mvc;

namespace IRO.Mvc.CoolSwagger
{
    [Route("[controller]/[action]")]
    [ApiExplorerSettings(IgnoreApi =true)]
    public class SwaggerClientController : ControllerBase
    {
        [HttpGet]
        public JsonResult Host()
        {
            var hostBase = "http://" + Request.Host.Value;
            return new JsonResult(hostBase);
        }

        [HttpGet]
        [Route("Script.js")]
        public JavaScriptResult SwaggerInjectedJs()
        {
            var script = @"
setTimeout(function(){
$(document).ready(function(){
  var swaggerJsonAnchorEl=$('hgroup').children('a').first();
  var swaggerJsonUrl=swaggerJsonAnchorEl[0].href;
  var a1=document.createElement('a');
  a1.href='/SwaggerClient/GenerateCSharp?swaggerJson='+swaggerJsonUrl;
  a1.innerHTML='<span class=\'url\'>GENERATE C# CLIENT</span>';
  swaggerJsonAnchorEl.after(a1);
  swaggerJsonAnchorEl.after('<br>');
  var a1=document.createElement('a');
  a1.href='/SwaggerClient/GenerateTypeScript?swaggerJson='+swaggerJsonUrl;
  a1.innerHTML='<span class=\'url\'>GENERATE TypeScript CLIENT</span>';
  swaggerJsonAnchorEl.after(a1);
  swaggerJsonAnchorEl.after('<br>');
});
}, 2000);
                ";
            return new JavaScriptResult(script);
        }

        [HttpGet]
        public string GenerateCSharp(string swaggerJson)
        {
            return "Not implemented now";
            return "1" + swaggerJson;
        }

        [HttpGet]
        public string GenerateTypeScript(string swaggerJson)
        {
            return "Not implemented now";
            return "2" + swaggerJson;
        }
    }
}
