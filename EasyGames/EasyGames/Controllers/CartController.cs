// Controllers/CartController.cs
using EasyGames.Data;
using EasyGames.Models;
using EasyGames.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EasyGames.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cart;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _users;

        public CartController(ICartService cart, ApplicationDbContext db, UserManager<ApplicationUser> users)
        { _cart = cart; _db = db; _users = users; }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Add(int productId, int qty = 1)
        {
            _cart.Add(productId, qty);
            return RedirectToAction(nameof(ViewCart));
        }

        public IActionResult ViewCart() => View(_cart.GetCart());

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult UpdateQty(int productId, int qty)
        {
            _cart.Update(productId, qty);
            return RedirectToAction(nameof(ViewCart));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            _cart.Remove(productId);
            return RedirectToAction(nameof(ViewCart));
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var items = _cart.GetCart();
            if (!items.Any()) return RedirectToAction(nameof(ViewCart));

            var ids = items.Select(i => i.ProductId).ToList();
            var products = _db.Products.Where(p => ids.Contains(p.Id)).ToList();
            foreach (var i in items)
            {
                var p = products.First(x => x.Id == i.ProductId);
                if (p.Stock < i.Qty)
                {
                    TempData["Error"] = $"Not enough stock for {p.Name}";
                    return RedirectToAction(nameof(ViewCart));
                }
            }

            using var tx = await _db.Database.BeginTransactionAsync();
            var order = new Order
            {
                UserId = _users.GetUserId(User)!,
                Total = items.Sum(x => x.Price * x.Qty),
                Items = items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Qty,
                    UnitPrice = i.Price
                }).ToList()
            };
            _db.Orders.Add(order);
            foreach (var i in items) products.First(x => x.Id == i.ProductId).Stock -= i.Qty;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            _cart.Clear();
            return RedirectToAction(nameof(Success));
        }

        public IActionResult Success() => View();
    }
}
