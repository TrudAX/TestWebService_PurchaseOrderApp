// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Models;

namespace PurchaseOrderApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PurchaseOrder>()
                .HasMany(po => po.Lines)
                .WithOne()
                .HasForeignKey(pol => pol.PurchId)
                .HasPrincipalKey(po => po.PurchId);
        }
    }
}
