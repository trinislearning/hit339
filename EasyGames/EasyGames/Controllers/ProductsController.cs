// Controllers/ProductsController.cs
using EasyGames.Data;
using EasyGames.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Controllers
{
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

        // GET: /Products
        public async Task<IActionResult> Index()
            => View(await _db.Products.OrderBy(p => p.Name).ToListAsync());

        // GET: /Products/Create
        public IActionResult Create() => View(new Product());

        // POST: /Products/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model, IFormFile? image)
        {
            if (!ModelState.IsValid) return View(model);

            // upload image -> save to wwwroot/uploads/products
            if (image is { Length: > 0 })
            {
                var saveResult = await SaveImageAsync(image);
                if (!saveResult.ok)
                {
                    ModelState.AddModelError(string.Empty, saveResult.error!);
                    return View(model);
                }
                model.ImageUrl = saveResult.url; // ví dụ: /uploads/products/xxx.webp
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

            // upload new image -> delete old pic and save new pic
            if (image is { Length: > 0 })
            {
                if (!string.IsNullOrWhiteSpace(dbItem.ImageUrl) && dbItem.ImageUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, dbItem.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
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
                // keep old image
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
                // delete local file
                if (!string.IsNullOrWhiteSpace(p.ImageUrl) && p.ImageUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                {
                    var path = Path.Combine(_env.WebRootPath, p.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
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
        private async Task<(bool ok, string? url, string? error)> SaveImageAsync(IFormFile file)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowed.Contains(ext))
                return (false, null, "Only .jpg, .jpeg, .png, .gif, .webp are allowed.");

            // opt image size ~ 5 MB
            if (file.Length > 5 * 1024 * 1024)
                return (false, null, "Image too large (max 5MB).");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);

            // return relative to ImageUrl
            return (true, $"/uploads/products/{fileName}", null);
        }
    }
}
