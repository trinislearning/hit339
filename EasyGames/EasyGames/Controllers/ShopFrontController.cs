using EasyGames.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Controllers
{
    public class ShopFrontController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ShopFrontController(ApplicationDbContext db) => _db = db;

        // 🏬 List of physical shops
        public async Task<IActionResult> Index()
        {
            var shops = await _db.Shops
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
            return View(shops);
        }

        // 🛍 Products available in each shop
        public async Task<IActionResult> Details(int id)
        {
            var shop = await _db.Shops
                .Include(s => s.Stocks)
                .ThenInclude(ss => ss.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shop == null) return NotFound();
            return View(shop);
        }
    }
}
