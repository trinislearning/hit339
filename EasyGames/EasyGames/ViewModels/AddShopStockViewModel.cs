namespace EasyGames.ViewModels
{
    // ViewModel for adding product quantity into a specific shop's stock
    public class AddShopStockViewModel
    {
        public int ShopId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        // For dropdowns
        public IEnumerable<(int Id, string Name)> Products { get; set; } = Enumerable.Empty<(int, string)>();
    }
}
