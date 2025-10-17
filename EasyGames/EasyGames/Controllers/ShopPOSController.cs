using EasyGames.Data;
using EasyGames.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Controllers
{
    [Authorize(Roles = "Shop,Owner")]
    public class ShopPOSController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ShopPOSController(ApplicationDbContext db) => _db = db;

        // GET: /ShopPOS?shopId=1
        public async Task<IActionResult> Index(int shopId)
        {
            var shop = await _db.Shops
                .Include(s => s.Stocks)
                .ThenInclude(ss => ss.Product)
                .FirstOrDefaultAsync(s => s.Id == shopId);

            if (shop == null)
                return NotFound();

            ViewBag.Shop = shop;
            return View(shop.Stocks);
        }

        // POST: /ShopPOS/Sell
        [HttpPost]
        public async Task<IActionResult> Sell(int stockId, int quantity, int shopId)
        {
            var stock = await _db.ShopStocks
                .Include(s => s.Product)
                .FirstOrDefaultAsync(s => s.Id == stockId);

            if (stock == null || quantity <= 0)
                return RedirectToAction(nameof(Index), new { shopId });

            if (stock.Quantity < quantity)
            {
                TempData["error"] = "Not enough stock!";
                return RedirectToAction(nameof(Index), new { shopId });
            }

            stock.Quantity -= quantity;

            // Optional: record the sale
            var sale = new PosSale
            {
                ShopId = shopId,
                ProductId = stock.ProductId,
                Quantity = quantity,
                Total = stock.Product.Price * quantity,
                SoldAt = DateTime.UtcNow
            };
            _db.Add(sale);

            await _db.SaveChangesAsync();
            TempData["ok"] = $"Sold {quantity} x {stock.Product.Name}.";
            return RedirectToAction(nameof(Index), new { shopId });
        }
    }
}
