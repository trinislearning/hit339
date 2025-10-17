using EasyGames.Data;
using EasyGames.Models;
using EasyGames.ViewModels;
using EasyGames.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace EasyGames.Controllers
{
    [Authorize(Roles = "Owner")]
    public class EmailsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly EmailService? _mailer;

        public EmailsController(ApplicationDbContext db, EmailService? mailer = null)
        {
            _db = db;
            _mailer = mailer;
        }

        // GET: /Emails/Compose?ok=...
        [HttpGet]
        public IActionResult Compose(string? ok = null)
        {
            // Query-string fallback message (in case TempData didn't survive)
            if (!string.IsNullOrEmpty(ok))
                ViewBag.Ok = WebUtility.UrlDecode(ok);

            return View(new ComposeEmailViewModel
            {
                Audience = "All",
                BodyHtml = "Hello everyone..."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Compose(ComposeEmailViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            IQueryable<ApplicationUser> usersQuery = _db.Users;

            switch (vm.Audience)
            {
                case "Tier":
                    usersQuery = string.IsNullOrWhiteSpace(vm.Tier)
                        ? usersQuery.Where(u => false)
                        : usersQuery.Where(u => u.Tier == vm.Tier);
                    break;

                case "Role":
                    if (!string.IsNullOrWhiteSpace(vm.Role))
                    {
                        usersQuery =
                            from u in _db.Users
                            join ur in _db.UserRoles on u.Id equals ur.UserId
                            join r in _db.Roles on ur.RoleId equals r.Id
                            where r.Name == vm.Role
                            select u;
                    }
                    else
                    {
                        usersQuery = usersQuery.Where(u => false);
                    }
                    break;

                case "Custom":
                    var emails = (vm.CustomEmails ?? string.Empty)
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(e => e.ToLower())
                        .ToList();

                    usersQuery = emails.Count == 0
                        ? usersQuery.Where(u => false)
                        : usersQuery.Where(u => u.Email != null && emails.Contains(u.Email.ToLower()));
                    break;

                default: break; // All
            }

            var recipients = await usersQuery
                .Where(u => u.Email != null)
                .Select(u => u.Email!)
                .Distinct()
                .ToListAsync();

            int recipientCount = recipients.Count;

            // Log to DB (assignment requirement)
            var log = new EmailLog
            {
                Subject = vm.Subject ?? string.Empty,
                Message = vm.BodyHtml ?? string.Empty,
                TargetGroup = vm.Audience ?? "All",
                RecipientCount = recipientCount,
                SentAt = DateTime.UtcNow
            };
            _db.EmailLogs.Add(log);
            await _db.SaveChangesAsync();

            // Optional actual send
            if (_mailer != null && recipientCount > 0 && !string.IsNullOrWhiteSpace(vm.Subject))
            {
                foreach (var to in recipients)
                {
                    try { await _mailer.SendAsync(to, vm.Subject, vm.BodyHtml ?? string.Empty); }
                    catch { /* ignore per-recipient failures */ }
                }
            }

            var who = vm.Audience switch
            {
                "Role" => $"role '{vm.Role}'",
                "Tier" => $"tier '{vm.Tier}'",
                "Custom" => "custom list",
                _ => "all users"
            };

            var msg = $"Email sent to {recipientCount} recipient(s) ({who}).";

            // Use TempData AND pass it via query-string as a backup
            TempData["ok"] = msg;
            return RedirectToAction(nameof(Compose), new { ok = WebUtility.UrlEncode(msg) });
        }

        public async Task<IActionResult> History()
        {
            var logs = await _db.EmailLogs
                                .OrderByDescending(e => e.SentAt)
                                .ToListAsync();
            return View(logs);
        }
    }
}
