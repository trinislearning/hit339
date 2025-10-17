namespace EasyGames.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EasyGames.Models;

    public interface IShopService
    {
        Task<Shop> CreateShopAsync(Shop shop);
        Task<List<Shop>> GetShopsAsync();
        Task<Shop?> GetShopAsync(int id);
        Task<bool> UpdateShopAsync(Shop shop);
        Task<bool> DeleteShopAsync(int id);

        Task<ShopStock> AddStockToShopAsync(int shopId, int productId, int quantity);
        Task<bool> ReduceShopStockAsync(int shopStockId, int quantity);
    }
}
