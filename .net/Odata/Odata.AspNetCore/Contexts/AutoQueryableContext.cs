using Microsoft.EntityFrameworkCore;
using Odata.AspNetCore.Entities;

namespace Odata.AspNetCore.Contexts
{
    public class AutoQueryableContext : DbContext
    {
        public AutoQueryableContext(DbContextOptions<AutoQueryableContext> options)
            : base(options)
        {
            
        }

        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<ProductCategory> ProductCategory { get; set; }
        public virtual DbSet<ProductModel> ProductModel { get; set; }
        public virtual DbSet<SalesOrderDetail> SalesOrderDetail { get; set; }
    }
}