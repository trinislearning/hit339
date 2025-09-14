// Data/IdentitySeed.cs
using System.Text.Json;
using EasyGames.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EasyGames.Data
{
    public static class IdentitySeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

            // 1) Roles
            foreach (var r in new[] { "Owner", "Customer" })
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));

            // 2) Owner user
            var email = "owner@easygames.local";
            var owner = await userMgr.FindByEmailAsync(email);
            if (owner == null)
            {
                owner = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = "Site Owner"
                };
                var ok = await userMgr.CreateAsync(owner, "Owner#123");
                if (ok.Succeeded) await userMgr.AddToRoleAsync(owner, "Owner");
            }

            // 3) Products (merge and add)
            var desired = new List<Product>();

            // prioritize reading wwwroot/data/products.json
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

            // Fallback only if there is no item to display
            if (desired.Count == 0)
            {
                desired = new List<Product>
                {
                    new Product {
                        Name = "Harry Potter and the Philosopher's Stone",
                        Category = "Book", Price = 22.99m, Stock = 50,
                        ImageUrl = "https://via.placeholder.com/900x650?text=Book", 
                        Description = "Classic fantasy novel."
                    },
                    new Product {
                        Name = "Catan Board Game",
                        Category = "Game", Price = 41.99m, Stock = 25,
                        ImageUrl = "https://via.placeholder.com/900x650?text=Game",
                        Description = "Strategy board game."
                    },
                    new Product {
                        Name = "Lego Race Car",
                        Category = "Toy", Price = 49.99m, Stock = 15,
                        ImageUrl = "https://via.placeholder.com/900x650?text=Toy",
                        Description = "Buildable toy car."
                    }
                };
            }

            // add products based on name
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
        }
    }
}
