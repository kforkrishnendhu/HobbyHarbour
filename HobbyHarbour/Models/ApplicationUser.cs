
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace HobbyHarbour.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }

        public string? OTP { get; set; }
    }

}

