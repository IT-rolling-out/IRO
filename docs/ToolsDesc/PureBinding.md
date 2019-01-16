# PureBinding

Свой биндер моделей.
Он позволяет получить параметры из json тела запроса, аналогично FromBody, но при этом не вынуждая создавать модель запроса.
Легче всего понять принцип его работы на примере PureBindingTestController.

```csharp
    //Здесь показан пример работы с библиотекой PureBinding. Работает она почти так же, как FromForm,
    //но при этом позволяет получить значения параметров из json, если он был передан в запросе.
    //Схожим образом работает FromBody, но для него нужно создавать модель передаваемого json объекта.

    [Route("api/pbtest/[action]")]
    [ApiController]
    public class PureBindingTestController : ControllerBase
    {
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
```
