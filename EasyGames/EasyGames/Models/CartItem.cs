using System;

namespace EasyGames.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }   // đóng băng giá lúc thêm
        public int Qty { get; set; }
        public string? ImageUrl { get; set; }
    }
}
