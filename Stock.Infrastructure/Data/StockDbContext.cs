using Microsoft.EntityFrameworkCore;
using Stock.Domain.Entities;

namespace Stock.Infrastructure.Data
{
    public class StockDbContext : DbContext
    {
        public StockDbContext(DbContextOptions<StockDbContext> options)
            : base(options)
        {
        }

        public DbSet<StockModel> Stocks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StockModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LocationId).IsRequired();
                entity.Property(e => e.ProductId).IsRequired();
                entity.Property(e => e.AddStock).IsRequired();
                entity.Property(e => e.LessStock).IsRequired();
                entity.Property(e => e.Purchase).IsRequired();
                entity.Property(e => e.Sales).IsRequired();
                entity.Property(e => e.Total).IsRequired();

                entity.HasIndex(e => new { e.LocationId, e.ProductId })
                      .IsUnique();
            });
        }

    }
}
