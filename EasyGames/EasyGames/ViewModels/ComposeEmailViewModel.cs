namespace EasyGames.ViewModels
{
    /// <summary>
    /// Form model for sending Owner bulk emails.
    /// Audience supports: All, ByTier, ByRole, Custom.
    /// </summary>
    public class ComposeEmailViewModel
    {
        public string Audience { get; set; } = "All"; // All | Tier | Role | Custom
        public string? Tier { get; set; }             // Bronze | Silver | Gold | Platinum
        public string? Role { get; set; }             // Owner | Customer | Shop (if you use roles)
        public string? CustomEmails { get; set; }     // comma-separated emails

        public string Subject { get; set; } = string.Empty;
        public string BodyHtml { get; set; } = "<p>Hello everyone,</p><p>...</p>";
    }
}
