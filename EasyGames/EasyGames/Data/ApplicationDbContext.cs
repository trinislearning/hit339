using EasyGames.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Data
{
    // IdentityDbContext<ApplicationUser> gives us ASP.NET Identity tables + ApplicationUser
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // --- Existing sets from Assignment 2 (keep them) ---
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        // --- NEW for Assignment 3 (Owner / Shopfront requirements) ---
        // Physical shops created and managed by the Owner
        public DbSet<Shop> Shops => Set<Shop>();

        // Per-shop inventory (must reference products from the Owner inventory)
        public DbSet<ShopStock> ShopStocks => Set<ShopStock>();

        // Simple audit of bulk emails sent by the Owner
        public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

    }
}
