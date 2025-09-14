using System.ComponentModel.DataAnnotations;

namespace EasyGames.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required, StringLength(120)] public string Name { get; set; }
        [Required] public string Category { get; set; }   // Book | Game | Toy
        [Range(0, 999999)] public decimal Price { get; set; }
        [Range(0, int.MaxValue)] public int Stock { get; set; }
        [Url] public string? ImageUrl { get; set; }
        [StringLength(1000)] public string? Description { get; set; }
    }
}
