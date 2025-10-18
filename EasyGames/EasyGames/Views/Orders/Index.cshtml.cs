using System.Collections.Generic;
using EasyGames.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EasyGames.Views.Orders
{
    public class IndexModel : PageModel
    {
        public List<Order> Orders { get; set; } = new();
    }
}
