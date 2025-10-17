using System.ComponentModel.DataAnnotations;

namespace EasyGames.Models
{
    public class ShopStock
    {
        public int Id { get; set; }
        [Required] public int ShopId { get; set; }
        [Required] public int ProductId { get; set; }

        [Range(0, 999999)]
        public int Quantity { get; set; }

        [StringLength(200)]
        public string ProductName { get; set; } = default!;
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }

        public int LowStockThreshold { get; set; } = 3;

        public Shop? Shop { get; set; }
        public Product? Product { get; set; }
    }
}
