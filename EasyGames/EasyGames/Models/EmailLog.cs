using System;
using System.ComponentModel.DataAnnotations;

namespace EasyGames.Models
{
    public class EmailLog
    {
        public int Id { get; set; }

        
        public string Subject { get; set; } = default!;

        // Body/content of the email
        
        public string Message { get; set; } = default!;

        // e.g. All / Bronze / Silver / Gold / Platinum
        public string TargetGroup { get; set; } = "All";

        // how many users we sent/logged for
        public int RecipientCount { get; set; }

        // when it was sent/logged
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public string? Audience { get; set; }

    }
}
