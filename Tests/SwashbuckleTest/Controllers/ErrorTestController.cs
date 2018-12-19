using ItRollingOut.MvcPart;
using ItRollingOut.PureBinding.Metadata;
using Microsoft.AspNetCore.Mvc;
using SwashbuckleTest.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwashbuckleTest.Controllers
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
