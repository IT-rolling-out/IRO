using System.Collections.Generic;
using System.Threading.Tasks;
using IRO.Mvc.CoolSwagger;
using IRO.Tests.SwashbuckleTest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IRO.Tests.SwashbuckleTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]    
    public class ValuesController : ControllerBase
    {
        /// <summary>
        /// sdsadas
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [SwaggerTagName("TagNameByAttr")]
        public async Task<IEnumerable<string>> Get([FromBody]RequestModel<object, int> requestModel)
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Oh hi mark
        /// </summary>
        /// <param name="id">Param comment</param>
        /// <param name="qwe">Param not exists</param>
        /// <returns>res comment</returns>
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {

            return "value";

        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }


    
}
