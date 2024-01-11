using System;
using System.ComponentModel.DataAnnotations;

namespace HobbyHarbour.ViewModels
{
	public class UserProfileViewModel
	{
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Phone]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be 10 digits")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }
}

