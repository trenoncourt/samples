using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Odata.AspNetCore.Contexts;
using Odata.AspNetCore.Entities;

namespace Odata.AspNetCore.Controllers
{
    public interface ICategoryService
    {
        IQueryable<Product> Products { get; set; }
    }
    
    [EnableQuery]
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        // GET api/values
        [HttpGet]
        public IQueryable<Product> Get([FromServices] AutoQueryableContext context)
        {
            return context.Product;
        }
    }
}