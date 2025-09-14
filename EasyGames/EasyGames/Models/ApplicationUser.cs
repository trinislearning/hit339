using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EasyGames.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData, StringLength(120)]
        public string? FullName { get; set; }

        [PersonalData, DataType(DataType.Date)]
        public DateTime? DOB { get; set; }
    }
}
