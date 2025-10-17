using System.ComponentModel.DataAnnotations;

namespace EasyGames.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; } = default!;

        [Required]
        public string Category { get; set; } = default!;   // Book | Game | Toy

        // SELL PRICE used on the storefront (was already 'Price' in A2)
        [Range(0, 999999)]
        public decimal Price { get; set; }

        // NEW: Owner financial fields
        // Cost price is needed to calculate profit/margin
        [Range(0, 999999)]
        public decimal CostPrice { get; set; }

        // Optional: where the stock came from (supplier/source)
        public string? Source { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Url]
        public string? ImageUrl { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }
    }
}
