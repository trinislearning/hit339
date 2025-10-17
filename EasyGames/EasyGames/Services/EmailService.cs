using System.Net;
using System.Net.Mail;
using EasyGames.Data;
using EasyGames.Models;

namespace EasyGames.Services
{
    /// <summary>
    /// Lightweight SMTP email sender for Owner bulk emails.
    /// Sends in small BCC batches to avoid huge TO lists.
    /// Logs each send into EmailLogs for auditing.
    /// </summary>
    public class EmailService
    {
        private readonly IConfiguration _cfg;
        private readonly ApplicationDbContext _db;

        public EmailService(IConfiguration cfg, ApplicationDbContext db)
        {
            _cfg = cfg;
            _db = db;
        }

        private SmtpClient CreateClient()
        {
            var host = _cfg["Smtp:Host"];
            var port = int.Parse(_cfg["Smtp:Port"] ?? "587");
            var enableSsl = bool.Parse(_cfg["Smtp:EnableSsl"] ?? "true");
            var user = _cfg["Smtp:User"];
            var pass = _cfg["Smtp:Pass"];

            return new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(user, pass)
            };
        }

        public async Task<int> SendBulkAsync(IEnumerable<string> recipients, string subject, string htmlBody, string audienceLabel)
        {
            var list = recipients
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!list.Any()) return 0;

            using var client = CreateClient();

            // send as BCC batches (e.g., 50 each) to reduce overhead
            const int batchSize = 50;
            for (int i = 0; i < list.Count; i += batchSize)
            {
                var batch = list.Skip(i).Take(batchSize).ToList();
                using var msg = new MailMessage
                {
                    From = new MailAddress(_cfg["Smtp:User"] ?? "noreply@easy.games"),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                // Put all recipients in BCC
                foreach (var r in batch)
                    msg.Bcc.Add(r);

                // Add a "friendly" To to avoid spam filters (some servers require non-empty To)
                msg.To.Add(_cfg["Smtp:User"] ?? "noreply@easy.games");

                await client.SendMailAsync(msg);
            }

            _db.EmailLogs.Add(new EmailLog
            {
                Subject = subject,
                Audience = audienceLabel,
                RecipientCount = list.Count,
                SentAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            return list.Count;
        }
    }
}
