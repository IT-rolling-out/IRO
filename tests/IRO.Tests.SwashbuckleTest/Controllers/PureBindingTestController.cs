using System;
using IRO.Mvc.Core;
using IRO.Mvc.PureBinding.Metadata;
using IRO.Tests.SwashbuckleTest.Models;
using Microsoft.AspNetCore.Mvc;

namespace IRO.Tests.SwashbuckleTest.Controllers
{
    //Здесь показан пример работы с библиотекой PureBinding. Работает она почти так же, как FromForm,
    //но при этом позволяет получить значения параметров из json, если он был передан в запросе.
    //Схожим образом работает FromBody, но для него нужно создавать модель передаваемого json объекта.

    [Route("pbtest/[action]")]
    [ApiController]
    public class PureBindingTestController : ControllerBase
    {
        [HttpGet]
        public Tuple<string, string> Get()
        {
            return null;
        }

        [HttpPost]
        public JsonResult TestPureBinding([FromPureBinding]string str, [FromPureBinding]int num)
        {
            var bodyText=HttpContext.GetRequestBodyText();
            return new JsonResult(new object[] { str, num , bodyText});
        }

        [HttpPost]
        public JsonResult TestFromBody([FromBody]PureBindingTestModel model)
        {
            return new JsonResult(model);
        }

        [HttpPost]
        public JsonResult TestFromForm([FromForm]string str, [FromForm]int num)
        {
            return new JsonResult(new object[] { str, num });
        }

    }
}