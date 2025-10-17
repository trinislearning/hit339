using System.ComponentModel.DataAnnotations;

namespace EasyGames.Models
{
    // Physical shop-front entity created by the Owner
    public class Shop
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string ShopName { get; set; } = default!;

        [StringLength(200)]
        public string? Location { get; set; }
    }
}
