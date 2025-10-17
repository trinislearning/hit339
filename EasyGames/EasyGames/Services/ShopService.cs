namespace EasyGames.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EasyGames.Data;
    using EasyGames.Models;
    using Microsoft.EntityFrameworkCore;

    public class ShopService : IShopService
    {
        private readonly ApplicationDbContext _db;
        public ShopService(ApplicationDbContext db) { _db = db; }

        public async Task<Shop> CreateShopAsync(Shop shop)
        {
            _db.Shops.Add(shop);
            await _db.SaveChangesAsync();
            return shop;
        }

        public Task<List<Shop>> GetShopsAsync() =>
            _db.Shops.AsNoTracking().ToListAsync();

        public Task<Shop?> GetShopAsync(int id) =>
            _db.Shops.Include(s => s.Stocks).FirstOrDefaultAsync(x => x.Id == id);

        public async Task<bool> UpdateShopAsync(Shop shop)
        {
            _db.Shops.Update(shop);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteShopAsync(int id)
        {
            var s = await _db.Shops.FindAsync(id);
            if (s == null) return false;
            _db.Shops.Remove(s);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<ShopStock> AddStockToShopAsync(int shopId, int productId, int quantity)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId)
                          ?? throw new InvalidOperationException("Product not found.");
            if (quantity < 0) throw new InvalidOperationException("Quantity must be >= 0.");

            product.Quantity = Math.Max(0, product.Quantity - quantity);

            var existing = await _db.ShopStocks
                .FirstOrDefaultAsync(ss => ss.ShopId == shopId && ss.ProductId == productId);

            if (existing == null)
            {
                existing = new ShopStock
                {
                    ShopId = shopId,
                    ProductId = productId,
                    Quantity = quantity,
                    ProductName = product.Name,
                    BuyPrice = product.BuyPrice,
                    SellPrice = product.SellPrice
                };
                _db.ShopStocks.Add(existing);
            }
            else
            {
                existing.Quantity += quantity;
            }

            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> ReduceShopStockAsync(int shopStockId, int quantity)
        {
            var ss = await _db.ShopStocks.FindAsync(shopStockId);
            if (ss == null) return false;
            ss.Quantity -= quantity;              // allow negative; controller warns
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
