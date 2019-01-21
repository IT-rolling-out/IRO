using IRO.Mvc.PureBinding.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace IRO.Tests.PureBindingsTest.Controllers
{
    //Здесь показан пример работы с библиотекой PureBinding. Работает она почти так же, как FromForm,
    //но при этом позволяет получить значения параметров из json, если он был передан в запросе.
    //Схожим образом работает FromBody, но для него нужно создавать модель передаваемого json объекта.

    [Route("pbtest/[action]")]
    [ApiController]
    public class PureBindingTestController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Must be post request to '/pbtest/TestPureBinding'.";
        }

        [HttpPost]
        public JsonResult TestPureBinding([FromPureBinding]string str, [FromPureBinding]int num)
        {
            return Test(str, num);
        }

        [HttpPost]
        public JsonResult TestFromBody([FromBody]PureBindingTestModel model)
        {
            return Test(model.str, model.num);
        }

        [HttpPost]
        public JsonResult TestFromForm([FromForm]string str, [FromForm]int num)
        {
            return Test(str, num);
        }

        JsonResult Test(string str, int num)
        {
            return new JsonResult(new object[] { str, num });
        }
    }
}