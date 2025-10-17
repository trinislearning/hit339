using EasyGames.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        // Shop module
        public DbSet<Shop> Shops => Set<Shop>();
        public DbSet<ShopStock> ShopStocks => Set<ShopStock>();

        public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

        // >>> NEW for POS + Promotions <<<
        public DbSet<PosSale> PosSales => Set<PosSale>();
        public DbSet<PosSaleItem> PosSaleItems => Set<PosSaleItem>();
        public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
        public DbSet<EmailCampaign> EmailCampaigns => Set<EmailCampaign>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // one stock row per (Shop, Product)
            b.Entity<ShopStock>()
                .HasIndex(x => new { x.ShopId, x.ProductId })
                .IsUnique();
        }
    }
}
