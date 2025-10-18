using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EasyGames.Data;
using EasyGames.Models;

namespace EasyGames.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _users;

        public OrdersController(ApplicationDbContext db, UserManager<ApplicationUser> users)
        {
            _db = db;
            _users = users;
        }

        // Customer: list own orders
        public async Task<IActionResult> Index()
        {
            var userId = _users.GetUserId(User);
            var orders = await _db.Orders
                                  .Where(o => o.UserId == userId)
                                  .Include(o => o.Items).ThenInclude(i => i.Product)
                                  .OrderByDescending(o => o.CreatedAt)
                                  .ToListAsync();
            return View(orders);
        }

        // Customer: view order details
        public async Task<IActionResult> Details(int id)
        {
            var userId = _users.GetUserId(User);
            var order = await _db.Orders
                                 .Where(o => o.UserId == userId && o.Id == id)
                                 .Include(o => o.Items).ThenInclude(i => i.Product)
                                 .FirstOrDefaultAsync();
            if (order == null) return NotFound();
            return View(order);
        }
    }
}
