using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyGames.Models
{
    public class Shop
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; } = default!;

        [StringLength(200)]
        public string? Location { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<ShopStock> Stocks { get; set; } = new List<ShopStock>();
        public ICollection<PosSale> Sales { get; set; } = new List<PosSale>();

        [NotMapped]
        public string ShopName
        {
            get => Name;
            set => Name = value;
        }
    }
}
