using System;
using System.Linq;
using System.Threading.Tasks;
using EasyGames.Data;
using EasyGames.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Controllers
{
    [Authorize(Roles = "Owner")]
    public class OwnerController : Controller
    {
        private readonly ApplicationDbContext _db;
        public OwnerController(ApplicationDbContext db) { _db = db; }

        public IActionResult Index() => View();

        // STOCK PAGE
        public async Task<IActionResult> Stock()
        {
            var products = await _db.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
            return View(products);
        }

        // SALES REPORT PAGE
        public async Task<IActionResult> SalesReport()
        {
            try
            {
                var sales = await _db.PosSales.AsNoTracking()
                                              .OrderByDescending(s => s.SoldAt)
                                              .Take(200)
                                              .ToListAsync();
                ViewBag.Total = sales.Sum(s => s.Total);
                return View(sales);
            }
            catch
            {
                ViewBag.Total = 0m;
                return View(Enumerable.Empty<PosSale>());
            }
        }

        // SEND EMAIL PAGE
        public IActionResult SendEmail() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmail(string subject, string body, string targetGroup = "All")
        {
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
            {
                TempData["Error"] = "Subject and Body are required.";
                return RedirectToAction(nameof(SendEmail));
            }

            try
            {
                var count = await _db.Users.CountAsync();

                var log = new EmailLog
                {
                    Subject = subject
                    // omit Message/TargetGroup/RecipientCount/SentAt if your EmailLog doesn't have them
                };
                _db.EmailLogs.Add(log);
                await _db.SaveChangesAsync();

                TempData["Message"] = $"Email logged for {count} users.";
            }
            catch
            {
                TempData["Error"] = "EmailLog table schema doesn't match. Either add the fields or simplify the log as above.";
            }

            return RedirectToAction(nameof(SendEmail));
        }

        // NEW: List orders for owner
        public async Task<IActionResult> Orders()
        {
            var orders = await _db.Orders
                                  .Include(o => o.User)
                                  .Include(o => o.Items).ThenInclude(i => i.Product)
                                  .OrderByDescending(o => o.CreatedAt)
                                  .ToListAsync();
            return View(orders);
        }

        // NEW: Order details for owner
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _db.Orders
                                 .Include(o => o.User)
                                 .Include(o => o.Items).ThenInclude(i => i.Product)
                                 .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }

        // NEW: Confirm an order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(int id)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.IsConfirmed = true;
            order.ConfirmedAt = DateTime.UtcNow;
            order.Status = "Confirmed";

            await _db.SaveChangesAsync();
            TempData["Message"] = $"Order #{order.Id} confirmed.";
            return RedirectToAction(nameof(Orders));
        }

        // NEW: Cancel an order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.IsConfirmed = false;
            order.Status = "Cancelled";

            await _db.SaveChangesAsync();
            TempData["Message"] = $"Order #{order.Id} cancelled.";
            return RedirectToAction(nameof(Orders));
        }

    }
}
