using System.Text.Json;
using EasyGames.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EasyGames.Data
{
    /// <summary>
    /// Seeds essential roles, default Owner account, and initial Products.
    /// Supports optional JSON import from wwwroot/data/products.json.
    /// </summary>
    public static class IdentitySeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

            // -----------------------
            // 1. Create essential roles
            // -----------------------
            var roles = new[] { "Owner", "Customer", "Shop" }; // "Shop" optional for POS module later
            foreach (var r in roles)
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));

            // -----------------------
            // 2. Ensure Owner account
            // -----------------------
            var email = "owner@easygames.local";
            var owner = await userMgr.FindByEmailAsync(email);
            if (owner == null)
            {
                owner = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = "Site Owner",
                    Tier = "Platinum"  // owner always platinum tier
                };

                var createResult = await userMgr.CreateAsync(owner, "Owner#123");
                if (createResult.Succeeded)
                    await userMgr.AddToRoleAsync(owner, "Owner");
            }
            else if (!await userMgr.IsInRoleAsync(owner, "Owner"))
            {
                await userMgr.AddToRoleAsync(owner, "Owner");
            }

            // -----------------------
            // 3. Seed initial products
            // -----------------------
            List<Product> desired = new();

            // Try reading from wwwroot/data/products.json
            var root = env.WebRootPath ?? env.ContentRootPath;
            var jsonPath = Path.Combine(root, "data", "products.json");

            if (File.Exists(jsonPath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(jsonPath);
                    desired = JsonSerializer.Deserialize<List<Product>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
                catch
                {
                    desired = new();
                }
            }

            // Fallback demo data if JSON file missing or empty
            if (desired.Count == 0)
            {
                desired = new List<Product>
                {
                    new Product {
                        Name = "Harry Potter and the Philosopher's Stone",
                        Category = "Book",
                        Price = 22.99m,
                        CostPrice = 10.50m,
                        Source = "Book Supplier Ltd.",
                        Stock = 50,
                        ImageUrl = "https://via.placeholder.com/900x650?text=Book",
                        Description = "Classic fantasy novel."
                    },
                    new Product {
                        Name = "Catan Board Game",
                        Category = "Game",
                        Price = 41.99m,
                        CostPrice = 20.00m,
                        Source = "Board Games Co.",
                        Stock = 25,
                        ImageUrl = "https://via.placeholder.com/900x650?text=Game",
                        Description = "Strategy board game."
                    },
                    new Product {
                        Name = "Lego Race Car",
                        Category = "Toy",
                        Price = 49.99m,
                        CostPrice = 25.00m,
                        Source = "Lego Distributor",
                        Stock = 15,
                        ImageUrl = "https://via.placeholder.com/900x650?text=Toy",
                        Description = "Buildable toy car."
                    }
                };
            }

            // Fill missing fields (CostPrice/Source) to make reports consistent
            foreach (var p in desired)
            {
                if (p.CostPrice <= 0)
                    p.CostPrice = Math.Round(p.Price * 0.6m, 2);
                if (string.IsNullOrWhiteSpace(p.Source))
                    p.Source = "Default Supplier";
            }

            // Add only if product name not already exists
            var existingNames = db.Products.Select(p => p.Name)
                                           .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var toAdd = desired
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .Where(p => !existingNames.Contains(p.Name))
                .ToList();

            if (toAdd.Count > 0)
            {
                db.Products.AddRange(toAdd);
                await db.SaveChangesAsync();
            }

            // -----------------------
            // 4. Optional: log seed summary
            // -----------------------
            Console.WriteLine($"[Seed] Roles: {string.Join(", ", roles)}");
            Console.WriteLine($"[Seed] Owner: {email}");
            Console.WriteLine($"[Seed] Products added: {toAdd.Count}");
        }
    }
}
