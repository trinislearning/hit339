using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace EasyGames.Services
{
    public class EmailService
    {
        private readonly IConfiguration _cfg;
        public EmailService(IConfiguration cfg) { _cfg = cfg; }

        public async Task SendAsync(string to, string subject, string html)
        {
            var host = _cfg["Smtp:Host"];
            var from = _cfg["Smtp:FromEmail"] ?? _cfg["Smtp:User"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
                return; // not configured → skip silently

            var port = int.TryParse(_cfg["Smtp:Port"], out var p) ? p : 587;
            var enable = bool.TryParse(_cfg["Smtp:EnableSsl"], out var e) ? e : true;
            var user = _cfg["Smtp:User"];
            var pass = _cfg["Smtp:Pass"];
            var fromName = _cfg["Smtp:FromName"] ?? "EasyGames";

            using var mail = new MailMessage(new MailAddress(from, fromName), new MailAddress(to))
            {
                Subject = subject ?? string.Empty,
                Body = html ?? string.Empty,
                IsBodyHtml = true
            };

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enable,
                Credentials = string.IsNullOrWhiteSpace(user)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(user, pass)
            };

            await client.SendMailAsync(mail);
        }
    }
}
