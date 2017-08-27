using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Nswag.NetCore.Api.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpPost()]
        public Test Get([FromBody] Test id)
        {
            return null;
        }

        //// POST api/values
        //[HttpPost]
        //public void Post([FromBody]string value)
        //{
        //}

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

    public class Test
    {
        /// <summary>
        /// Test de commentaire principal.
        /// </summary>
        public Toto Toto { get; set; }

        
        public DateTime? MyProperty { get; set; }

        public string Titi { get; set; }


        public int Tutu { get; set; }
    }

    public enum Toto
    {
        /// <summary>
        /// Test de commentaire.
        /// </summary>
        T = 1,
        U = 2
    }
}
