using System.ComponentModel.DataAnnotations;

namespace EasyGames.Models
{
    public enum CustomerTier { Bronze, Silver, Gold, Platinum }

    public class CustomerProfile
    {
        public int Id { get; set; }
        [Required] public string UserId { get; set; } = default!;
        [Phone] public string? Phone { get; set; }
        public decimal LifetimeSpend { get; set; }
        public CustomerTier Tier { get; set; } = CustomerTier.Bronze;

        public ApplicationUser? User { get; set; }
    }
}
