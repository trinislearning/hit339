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
        public async Task<IActionResult> SalesByUser(DateTime? from, DateTime? to)
        {
            var start = from ?? DateTime.UtcNow.AddMonths(-1);
            var end = (to ?? DateTime.UtcNow).AddDays(1); // make 'to' inclusive

            // ⚙️ Fix: force client-side eval using AsEnumerable()
            var rows = _db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Order.User)
                .AsEnumerable()  // ✅ SQLite-safe
                .Where(oi => oi.Order.CreatedAt >= start && oi.Order.CreatedAt < end)
                .GroupBy(oi => new { oi.Order.UserId, oi.Order.User.Email })
                .Select(g => new SalesByUserVm
                {
                    UserId = g.Key.UserId ?? string.Empty,
                    Email = g.Key.Email ?? string.Empty,
                    Orders = g.Select(x => x.OrderId).Distinct().Count(),
                    TotalQty = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => (decimal)x.UnitPrice * x.Quantity),
                    Profit = g.Sum(x => ((decimal)x.UnitPrice - (decimal)x.UnitCost) * x.Quantity)
                })
                .OrderByDescending(x => x.Profit)
                .ToList();

            // Update user tiers safely
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

        // Tier thresholds – adjust if needed
        private static string GetTierFromProfit(decimal profit)
        {
            if (profit >= 5000) return "Platinum";
            if (profit >= 2000) return "Gold";
            if (profit >= 500) return "Silver";
            return "Bronze";
        }
    }

    // ViewModel
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
