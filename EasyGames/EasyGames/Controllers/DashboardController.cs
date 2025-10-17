using EasyGames.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Controllers
{
    /// <summary>
    /// Owner dashboard with monthly KPIs and quick views.
    /// </summary>
    [Authorize(Roles = "Owner")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        public DashboardController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var start = DateTime.UtcNow.Date.AddDays(-30);
            var end = DateTime.UtcNow.Date.AddDays(1);

            var items = await _db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Where(oi => oi.Order.CreatedAt >= start && oi.Order.CreatedAt < end)
                .ToListAsync();

            var revenue = items.Sum(x => x.UnitPrice * x.Quantity);
            var profit = items.Sum(x => (x.UnitPrice - x.UnitCost) * x.Quantity);

            // Low stock (owner inventory)
            var lowStock = await _db.Products
                .Where(p => p.Stock < 5)
                .OrderBy(p => p.Stock).ThenBy(p => p.Name)
                .Take(10).ToListAsync();

            // Top 5 products by profit in period
            var top = items
                .GroupBy(x => new { x.ProductId, x.Product.Name })
                .Select(g => new TopProductVm
                {
                    ProductId = g.Key.ProductId,
                    Name = g.Key.Name,
                    Qty = g.Sum(x => x.Quantity),
                    Profit = g.Sum(x => (x.UnitPrice - x.UnitCost) * x.Quantity)
                })
                .OrderByDescending(x => x.Profit)
                .Take(5).ToList();

            var vm = new DashboardVm
            {
                From = start,
                To = end.AddDays(-1),
                Revenue30d = revenue,
                Profit30d = profit,
                LowStock = lowStock.Select(p => new LowStockVm
                {
                    Id = p.Id,
                    Name = p.Name,
                    Qty = p.Stock
                }).ToList(),
                TopProfit = top
            };
            return View(vm);
        }
    }

    public class DashboardVm
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public decimal Revenue30d { get; set; }
        public decimal Profit30d { get; set; }
        public List<LowStockVm> LowStock { get; set; } = new();
        public List<TopProductVm> TopProfit { get; set; } = new();
    }
    public class LowStockVm { public int Id { get; set; } public string Name { get; set; } = ""; public int Qty { get; set; } }
    public class TopProductVm { public int ProductId { get; set; } public string Name { get; set; } = ""; public int Qty { get; set; } public decimal Profit { get; set; } }
}
