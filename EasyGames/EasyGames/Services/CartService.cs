using System.Collections.Generic;
using System.Linq;
using EasyGames.Data;
using EasyGames.Extensions;
using EasyGames.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Services
{
    public class CartService : ICartService
    {
        private const string KEY = "CART";
        private readonly IHttpContextAccessor _http;
        private readonly ApplicationDbContext _db;

        public CartService(IHttpContextAccessor http, ApplicationDbContext db)
        {
            _http = http;
            _db = db;
        }

        private ISession Ses => _http.HttpContext!.Session;

        public List<CartItem> GetCart() =>
            Ses.GetObject<List<CartItem>>(KEY) ?? new List<CartItem>();

        private void Save(List<CartItem> cart) => Ses.SetObject(KEY, cart);

        public int GetCount() => GetCart().Sum(x => x.Qty);

        public void Add(int productId, int qty = 1)
        {
            var cart = GetCart();
            var existing = cart.FirstOrDefault(x => x.ProductId == productId);
            if (existing == null)
            {
                var p = _db.Products.AsNoTracking().FirstOrDefault(x => x.Id == productId)
                        ?? throw new System.Exception("Product not found");
                cart.Add(new CartItem
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Qty = qty,
                    ImageUrl = p.ImageUrl
                });
            }
            else
            {
                existing.Qty += qty;
            }
            Save(cart);
        }

        public void Update(int productId, int qty)
        {
            var cart = GetCart();
            var it = cart.FirstOrDefault(x => x.ProductId == productId);
            if (it == null) return;

            if (qty <= 0) cart.Remove(it);
            else it.Qty = qty;

            Save(cart);
        }

        public void Remove(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(x => x.ProductId == productId);
            Save(cart);
        }

        public void Clear() => Save(new List<CartItem>());

        public decimal GetSubtotal() => GetCart().Sum(x => x.Price * x.Qty);
    }
}
