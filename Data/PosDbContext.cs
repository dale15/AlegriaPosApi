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

            modelBuilder.Entity<ProductMaterial>()
                .HasIndex(pm => new { pm.ProductId, pm.MaterialId })
                .IsUnique();

            modelBuilder.Entity<ProductMaterial>()
                .Property(pm => pm.QuantityUsed)
                .HasPrecision(18, 4);
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductModifier> ProductModifiers => Set<ProductModifier>();
        public DbSet<ProductModifierOption> ProductModifierOptions => Set<ProductModifierOption>();
        public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
        public DbSet<Material> Materials => Set<Material>();
        public DbSet<MaterialStockLog> MaterialStockLogs => Set<MaterialStockLog>();
        public DbSet<ProductMaterial> ProductMaterials => Set<ProductMaterial>();
        public DbSet<Sale> Sale => Set<Sale>();
    }
}
