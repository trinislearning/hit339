using System.Collections.Generic;

namespace EasyGames.Services
{
    public interface ICartService
    {
        List<EasyGames.Models.CartItem> GetCart();
        int GetCount();
        void Add(int productId, int qty = 1);
        void Update(int productId, int qty);
        void Remove(int productId);
        void Clear();
        decimal GetSubtotal();
    }
}
