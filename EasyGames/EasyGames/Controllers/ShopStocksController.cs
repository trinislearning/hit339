using EasyGames.Data;
using EasyGames.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Controllers
{
    [Authorize(Roles = "Shop,Owner")]
    public class ShopStocksController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ShopStocksController(ApplicationDbContext db) => _db = db;

        // GET: /ShopStocks?shopId=1
        public async Task<IActionResult> Index(int shopId)
        {
            var shop = await _db.Shops
                .Include(s => s.Stocks)
                    .ThenInclude(ss => ss.Product)
                .FirstOrDefaultAsync(s => s.Id == shopId);

            if (shop == null) return NotFound();

            ViewBag.Shop = shop;
            return View(shop.Stocks.OrderBy(s => s.ProductName).ToList());
        }

        // GET: /ShopStocks/Add?shopId=1
        public async Task<IActionResult> Add(int shopId)
        {
            var products = await _db.Products.OrderBy(p => p.Name).ToListAsync();
            ViewBag.ShopId = shopId;
            ViewBag.Products = products;
            return View();
        }

        // POST: /ShopStocks/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int shopId, int productId, int quantity)
        {
            if (quantity < 0) quantity = 0;

            var shop = await _db.Shops.FindAsync(shopId);
            var product = await _db.Products.FindAsync(productId);
            if (shop == null || product == null) return NotFound();

            var existing = await _db.ShopStocks
                .FirstOrDefaultAsync(s => s.ShopId == shopId && s.ProductId == productId);

            if (existing != null)
            {
                // Update existing row
                existing.Quantity += quantity;
                existing.ProductName = product.Name;     // keep mirrored fields in sync
                existing.BuyPrice = product.CostPrice;
                existing.SellPrice = product.Price;
            }
            else
            {
                // Create new row (IMPORTANT: set ProductName/BuyPrice/SellPrice)
                _db.ShopStocks.Add(new ShopStock
                {
                    ShopId = shopId,
                    ProductId = productId,
                    Quantity = quantity,
                    ProductName = product.Name,           // NOT NULL in DB
                    BuyPrice = product.CostPrice,
                    SellPrice = product.Price
                });
            }

            await _db.SaveChangesAsync();
            TempData["ok"] = "Stock updated successfully.";
            return RedirectToAction(nameof(Index), new { shopId });
        }

        // OPTIONAL: Bulk import all owner products to a shop with default qty
        // POST: /ShopStocks/ImportAll?shopId=1&qty=10
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportAll(int shopId, int qty = 10)
        {
            if (qty < 1) qty = 1;

            var shop = await _db.Shops
                .Include(s => s.Stocks)
                .FirstOrDefaultAsync(s => s.Id == shopId);
            if (shop == null) return NotFound();

            var existingIds = shop.Stocks.Select(ss => ss.ProductId).ToHashSet();

            var productsToAdd = await _db.Products
                .Where(p => !existingIds.Contains(p.Id))
                .ToListAsync();

            foreach (var p in productsToAdd)
            {
                _db.ShopStocks.Add(new ShopStock
                {
                    ShopId = shopId,
                    ProductId = p.Id,
                    Quantity = qty,
                    ProductName = p.Name,           // NOT NULL
                    BuyPrice = p.CostPrice,
                    SellPrice = p.Price
                });
            }

            await _db.SaveChangesAsync();
            TempData["ok"] = $"Imported {productsToAdd.Count} products (qty {qty}).";
            return RedirectToAction(nameof(Index), new { shopId });
        }

        // POST: /ShopStocks/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int shopId)
        {
            var item = await _db.ShopStocks.FindAsync(id);
            if (item != null)
            {
                _db.ShopStocks.Remove(item);
                await _db.SaveChangesAsync();
                TempData["ok"] = "Stock removed.";
            }
            return RedirectToAction(nameof(Index), new { shopId });
        }
    }
}
