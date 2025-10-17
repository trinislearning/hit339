using System;
using System.ComponentModel.DataAnnotations;

namespace EasyGames.Models
{
    public class EmailCampaign
    {
        public int Id { get; set; }
        [Required, StringLength(160)] public string Subject { get; set; } = default!;
        [Required] public string HtmlBody { get; set; } = default!;
        public CustomerTier? TargetTier { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public int Recipients { get; set; }
        public int Sent { get; set; }
    }
}
