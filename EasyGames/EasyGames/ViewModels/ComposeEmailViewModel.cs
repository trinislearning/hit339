namespace EasyGames.ViewModels
{
    public class ComposeEmailViewModel
    {
        public string Audience { get; set; } = "All"; // All | Tier | Role | Custom
        public string? Tier { get; set; }
        public string? Role { get; set; }
        public string? CustomEmails { get; set; }

        public string Subject { get; set; } = string.Empty;

        // Plain text default (NO <p> tags)
        public string BodyHtml { get; set; } = "Hello everyone...";
    }
}
