using EasyGames.Data;
using EasyGames.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Controllers
{
    // Simple CRUD for physical shops managed by the Owner
    [Authorize(Roles = "Owner")]
    public class OwnerShopsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public OwnerShopsController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
            => View(await _db.Shops.OrderBy(s => s.ShopName).ToListAsync());

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Shop s)
        {
            if (!ModelState.IsValid) return View(s);
            _db.Shops.Add(s);
            await _db.SaveChangesAsync();
            TempData["ok"] = "Shop created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var s = await _db.Shops.FindAsync(id);
            if (s == null) return NotFound();
            return View(s);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Shop s)
        {
            if (!ModelState.IsValid) return View(s);
            _db.Shops.Update(s);
            await _db.SaveChangesAsync();
            TempData["ok"] = "Shop updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var s = await _db.Shops.FindAsync(id);
            if (s != null)
            {
                _db.Shops.Remove(s);
                await _db.SaveChangesAsync();
                TempData["ok"] = "Shop deleted.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
