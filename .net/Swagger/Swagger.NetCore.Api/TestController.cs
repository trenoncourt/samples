using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Swagger.NetCore.Api
{
    [Route("test")]
    public class TestController
    {
        [HttpPost]
        public void Post([FromBody, Required] PostRequest request)
        {
            
        }

        public class PostRequest
        {
            public MyEnum MyEnum { get; set; }
        }

        public enum MyEnum
        {
            V1 = 1,
            V2 = 2
        }
    }
}