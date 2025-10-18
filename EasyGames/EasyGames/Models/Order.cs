using System.ComponentModel.DataAnnotations.Schema;

namespace EasyGames.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal Total { get; set; }
        public List<OrderItem> Items { get; set; } = new();

        // New: order state so owner can confirm/receive orders
        public bool IsConfirmed { get; set; } = false;
        public DateTime? ConfirmedAt { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
