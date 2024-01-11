using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HobbyHarbour.Models
{
    public class Address
    {
        public int AddressID { get; set; }

        public string UserID { get; set; }

        // Define the foreign key relationships
        [ForeignKey("UserID")]
        public ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "FullName is required.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Street Address is required.")]
        public string StreetAddress { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required.")]
        public string State { get; set; }

        [Required(ErrorMessage = "Postal Code is required.")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Invalid phone number. Please enter a 10-digit number.")]
        public string PhoneNumber { get; set; }

    }

}

