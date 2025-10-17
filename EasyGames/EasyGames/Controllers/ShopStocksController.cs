using EasyGames.Data;
using EasyGames.Models;
using EasyGames.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Controllers
{
    // Owner manages per-shop inventory; shop owners would use this in a real system
    [Authorize(Roles = "Owner")]
    public class ShopStocksController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ShopStocksController(ApplicationDbContext db) => _db = db;

        // GET: /ShopStocks?shopId=1
        // Lists current stock for a shop
        public async Task<IActionResult> Index(int shopId)
        {
            var shop = await _db.Shops.FindAsync(shopId);
            if (shop == null) return NotFound();

            ViewBag.Shop = shop;
            var rows = await _db.ShopStocks
                .Include(ss => ss.Product)
                .Where(ss => ss.ShopId == shopId)
                .OrderBy(ss => ss.Product.Name)
                .ToListAsync();

            return View(rows);
        }

        // GET: /ShopStocks/Add?shopId=1
        // Add products into a shop's inventory from Owner's product list
        public async Task<IActionResult> Add(int shopId)
        {
            var shop = await _db.Shops.FindAsync(shopId);
            if (shop == null) return NotFound();

            var vm = new AddShopStockViewModel
            {
                ShopId = shopId,
                Products = await _db.Products
                    .OrderBy(p => p.Name)
                    .Select(p => new ValueTuple<int, string>(p.Id, p.Name))
                    .ToListAsync()
            };
            ViewBag.Shop = shop;
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddShopStockViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var shop = await _db.Shops.FindAsync(vm.ShopId);
            if (shop == null) return NotFound();

            // Ensure product exists in Owner inventory
            var product = await _db.Products.FindAsync(vm.ProductId);
            if (product == null)
            {
                ModelState.AddModelError("", "Product not found.");
                return View(vm);
            }

            // Create or update shop stock row
            var existing = await _db.ShopStocks
                .FirstOrDefaultAsync(x => x.ShopId == vm.ShopId && x.ProductId == vm.ProductId);

            if (existing == null)
            {
                _db.ShopStocks.Add(new ShopStock
                {
                    ShopId = vm.ShopId,
                    ProductId = vm.ProductId,
                    Quantity = Math.Max(0, vm.Quantity)
                });
            }
            else
            {
                existing.Quantity += Math.Max(0, vm.Quantity);
            }

            await _db.SaveChangesAsync();
            TempData["ok"] = "Stock added to shop.";
            return RedirectToAction(nameof(Index), new { shopId = vm.ShopId });
        }

        // POST: /ShopStocks/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var row = await _db.ShopStocks.FindAsync(id);
            if (row != null)
            {
                var shopId = row.ShopId;
                _db.ShopStocks.Remove(row);
                await _db.SaveChangesAsync();
                TempData["ok"] = "Item removed.";
                return RedirectToAction(nameof(Index), new { shopId });
            }
            return RedirectToAction("Index", "OwnerShops");
        }
    }
}
