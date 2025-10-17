using System.ComponentModel.DataAnnotations;

namespace EasyGames.Models
{
    // Inventory per physical shop (must reference products from the Owner inventory)
    public class ShopStock
    {
        public int Id { get; set; }

        [Required]
        public int ShopId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        public Shop Shop { get; set; } = default!;
        public Product Product { get; set; } = default!;
    }
}
