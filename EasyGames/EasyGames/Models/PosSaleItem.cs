using System.ComponentModel.DataAnnotations;

namespace EasyGames.Models
{
    public class PosSaleItem
    {
        public int Id { get; set; }
        [Required] public int PosSaleId { get; set; }
        [Required] public int ShopStockId { get; set; }
        [Required, StringLength(200)] public string ProductName { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }

        public PosSale? Sale { get; set; }
    }
}
