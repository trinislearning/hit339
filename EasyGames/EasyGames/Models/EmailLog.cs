namespace EasyGames.Models
{
    // Simple audit log for Owner's bulk emails
    public class EmailLog
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Audience { get; set; } = "All";  // e.g., All, Tier=Gold, Role=Customer
        public int RecipientCount { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
