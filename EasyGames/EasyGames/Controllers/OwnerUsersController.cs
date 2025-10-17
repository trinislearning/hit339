using EasyGames.Data;
using EasyGames.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Controllers
{
    /// <summary>
    /// Owner can list users and adjust their Tier.
    /// (We don't change passwords or Identity roles here.)
    /// </summary>
    [Authorize(Roles = "Owner")]
    public class OwnerUsersController : Controller
    {
        private readonly ApplicationDbContext _db;
        public OwnerUsersController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? q, string? tier)
        {
            var users = _db.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                users = users.Where(u =>
                    (u.Email != null && u.Email.Contains(q)) ||
                    (u.FullName != null && u.FullName.Contains(q)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(q)));
            }
            if (!string.IsNullOrWhiteSpace(tier))
            {
                users = users.Where(u => u.Tier == tier);
            }

            var list = await users
                .OrderBy(u => u.Email)
                .Select(u => new OwnerUserVm
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
                    FullName = u.FullName ?? "",
                    Phone = u.PhoneNumber ?? "",
                    Tier = u.Tier
                }).ToListAsync();

            ViewBag.Tier = tier ?? "";
            ViewBag.Query = q ?? "";
            return View(list);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (u == null) return NotFound();
            return View(new OwnerUserEditVm
            {
                Id = u.Id,
                Email = u.Email ?? "",
                FullName = u.FullName ?? "",
                Phone = u.PhoneNumber ?? "",
                Tier = u.Tier
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(OwnerUserEditVm vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == vm.Id);
            if (u == null) return NotFound();

            // Only update editable business fields here
            u.FullName = vm.FullName;
            u.PhoneNumber = vm.Phone;
            u.Tier = vm.Tier;

            await _db.SaveChangesAsync();
            TempData["ok"] = "User updated.";
            return RedirectToAction(nameof(Index));
        }
    }

    public class OwnerUserVm
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Tier { get; set; } = "Bronze";
    }

    public class OwnerUserEditVm
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Tier { get; set; } = "Bronze";
    }
}
