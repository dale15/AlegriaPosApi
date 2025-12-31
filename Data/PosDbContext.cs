using AlegriaPosApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AlegriaPosApi.Data
{
    public class PosDbContext : DbContext
    {
        public PosDbContext(DbContextOptions<PosDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName()!.ToLower());
            }
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductModifier> ProductModifiers => Set<ProductModifier>();
        public DbSet<ProductModifierOption> ProductModifierOptions => Set<ProductModifierOption>();
        public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    }
}
