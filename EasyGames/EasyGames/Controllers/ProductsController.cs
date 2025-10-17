// Controllers/ProductsController.cs
using EasyGames.Data;
using EasyGames.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Controllers
{
    /// <summary>
    /// Owner-only product management with image upload support.
    /// Extended with search/sort + margin view (uses Product.CostPrice and Price).
    /// </summary>
    [Authorize(Roles = "Owner")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductsController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: /Products?q=...&sort=margin|qty|name
        // Owner list with search and sort, keeping your original simple list as default.
        public async Task<IActionResult> Index(string? q, string? sort)
        {
            IQueryable<Product> items = _db.Products;

            // Text search across Name/Category/Source
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                items = items.Where(p =>
                    p.Name.Contains(term) ||
                    p.Category.Contains(term) ||
                    (p.Source != null && p.Source.Contains(term)));
            }

            // Sorting
            items = (sort ?? "name").ToLowerInvariant() switch
            {
                "qty" => items.OrderByDescending(p => p.Stock).ThenBy(p => p.Name),
                "margin" => items.OrderByDescending(p => p.Price > 0 ? (p.Price - p.CostPrice) / p.Price : 0)
                                   .ThenBy(p => p.Name),
                _ => items.OrderBy(p => p.Name)
            };

            var list = await items.ToListAsync();
            ViewBag.Query = q ?? "";
            ViewBag.Sort = sort ?? "name";
            return View(list);
        }

        // GET: /Products/Create
        public IActionResult Create() => View(new Product());

        // POST: /Products/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model, IFormFile? image)
        {
            if (!ModelState.IsValid) return View(model);

            // Upload image -> save to wwwroot/uploads/products
            if (image is { Length: > 0 })
            {
                var saveResult = await SaveImageAsync(image);
                if (!saveResult.ok)
                {
                    ModelState.AddModelError(string.Empty, saveResult.error!);
                    return View(model);
                }
                model.ImageUrl = saveResult.url; // e.g. /uploads/products/xxx.webp|jpg|png...
            }

            _db.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _db.Products.FindAsync(id);
            return p == null ? NotFound() : View(p);
        }

        // POST: /Products/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product model, IFormFile? image)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var dbItem = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (dbItem == null) return NotFound();

            // Upload new image -> delete old file if stored under /uploads/
            if (image is { Length: > 0 })
            {
                if (!string.IsNullOrWhiteSpace(dbItem.ImageUrl) &&
                    dbItem.ImageUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                {
                    var oldPath = Path.Combine(_env.WebRootPath,
                        dbItem.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                var saveResult = await SaveImageAsync(image);
                if (!saveResult.ok)
                {
                    ModelState.AddModelError(string.Empty, saveResult.error!);
                    return View(model);
                }
                model.ImageUrl = saveResult.url;
            }
            else
            {
                // Keep old image if no new file uploaded
                model.ImageUrl = dbItem.ImageUrl;
            }

            _db.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _db.Products.FindAsync(id);
            return p == null ? NotFound() : View(p);
        }

        // POST: /Products/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p != null)
            {
                // Delete local file if it was stored under /uploads/
                if (!string.IsNullOrWhiteSpace(p.ImageUrl) &&
                    p.ImageUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                {
                    var path = Path.Combine(_env.WebRootPath,
                        p.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }

                _db.Products.Remove(p);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // (optional) /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var p = await _db.Products.FindAsync(id);
            return p == null ? NotFound() : View(p);
        }

        // ===== Helpers =====
        /// <summary>
        /// Saves an uploaded image under wwwroot/uploads/products and returns a relative URL.
        /// Allows .jpg, .jpeg, .png, .gif, .webp up to 5MB.
        /// </summary>
        private async Task<(bool ok, string? url, string? error)> SaveImageAsync(IFormFile file)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowed.Contains(ext))
                return (false, null, "Only .jpg, .jpeg, .png, .gif, .webp are allowed.");

            // Max ~5MB
            if (file.Length > 5 * 1024 * 1024)
                return (false, null, "Image too large (max 5MB).");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);

            // Return relative URL to be stored in Product.ImageUrl
            return (true, $"/uploads/products/{fileName}", null);
        }
    }
}
