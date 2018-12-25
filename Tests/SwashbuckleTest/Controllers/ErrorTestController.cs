using System.Threading.Tasks;
using IRO_Tests.SwashbuckleTest.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace IRO_Tests.SwashbuckleTest.Controllers
{
    [Route("err/[action]")]
    [ApiController]
    public class ErrorTestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Test1()
        {
            return BadRequest();
        }

        [HttpGet]
        public IActionResult Test2()
        {
            throw new ClientException("Exceptions here!");
        }

        [HttpGet]
        public void Test3()
        {
            Task.Run(() =>
            {
                throw new ClientException();
            }).Wait();
        }
    }
}
