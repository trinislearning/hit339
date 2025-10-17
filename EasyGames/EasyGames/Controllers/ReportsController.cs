using EasyGames.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Controllers
{
    // Owner-facing reporting controller
    [Authorize(Roles = "Owner")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ReportsController(ApplicationDbContext db) => _db = db;

        // GET: /Reports/SalesByUser?from=2025-01-01&to=2025-12-31
        // Aggregates revenue/profit from OrderItems. Assumes:
        // - Order has CreatedAt and User navigation (Identity)
        // - OrderItem has UnitPrice (sell) and UnitCost (cost)
        public async Task<IActionResult> SalesByUser(DateTime? from, DateTime? to)
        {
            var start = from ?? DateTime.UtcNow.AddMonths(-1);
            var end = (to ?? DateTime.UtcNow).AddDays(1); // make 'to' inclusive

            var rows = await _db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Order.User)
                .Where(oi => oi.Order.CreatedAt >= start && oi.Order.CreatedAt < end)
                .GroupBy(oi => new { oi.Order.UserId, oi.Order.User.Email })
                .Select(g => new SalesByUserVm
                {
                    UserId = g.Key.UserId!,
                    Email = g.Key.Email!,
                    Orders = g.Select(x => x.OrderId).Distinct().Count(),
                    TotalQty = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.UnitPrice * x.Quantity),
                    Profit = g.Sum(x => (x.UnitPrice - x.UnitCost) * x.Quantity)
                })
                .OrderByDescending(x => x.Profit)
                .ToListAsync();

            // Optional: auto-update tier on users, based on computed profit in the date range
            var ids = rows.Select(r => r.UserId).Distinct().ToList();
            var users = await _db.Users.Where(u => ids.Contains(u.Id)).ToListAsync();
            foreach (var r in rows)
            {
                var u = users.FirstOrDefault(x => x.Id == r.UserId);
                if (u is null) continue;
                var newTier = GetTierFromProfit(r.Profit);
                if (!string.Equals(u.Tier, newTier, StringComparison.OrdinalIgnoreCase))
                    u.Tier = newTier;
            }
            await _db.SaveChangesAsync();

            ViewBag.From = start;
            ViewBag.To = end.AddDays(-1);
            return View(rows);
        }

        // Tier thresholds – adjust to your team’s decision if needed.
        private static string GetTierFromProfit(decimal profit)
        {
            if (profit >= 5000) return "Platinum";
            if (profit >= 2000) return "Gold";
            if (profit >= 500) return "Silver";
            return "Bronze";
        }
    }

    // Lightweight view model for the report table
    public class SalesByUserVm
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Orders { get; set; }
        public int TotalQty { get; set; }
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }

        public string Tier =>
            Profit >= 5000 ? "Platinum" :
            Profit >= 2000 ? "Gold" :
            Profit >= 500 ? "Silver" : "Bronze";
    }
}
