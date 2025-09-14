using EasyGames.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ShopController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? category, string? q)
        {
            var query = _db.Products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(category)) query = query.Where(x => x.Category == category);
            if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.Name.Contains(q));
            return View(await query.OrderBy(x => x.Name).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var p = await _db.Products.FindAsync(id);
            return p == null ? NotFound() : View(p);
        }
    }
}
