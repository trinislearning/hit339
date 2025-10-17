namespace EasyGames.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = default!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = default!;

        public int Quantity { get; set; }

        // Snapshot of the selling price at the time of purchase
        public decimal UnitPrice { get; set; }

        // NEW: Snapshot of the cost (so profit reports remain correct even if Product.CostPrice changes later)
        public decimal UnitCost { get; set; }
    }
}
