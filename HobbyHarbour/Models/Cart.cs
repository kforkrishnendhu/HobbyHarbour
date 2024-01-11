using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HobbyHarbour.Models
{
    public class Cart
    {
        [Key]
        public int CartID { get; set; }

        [Required]
        public string UserID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }

        // Define the foreign key relationships
        [ForeignKey("UserID")]
        public ApplicationUser? User { get; set; }

        [ForeignKey("ProductID")]
        public Product? Product { get; set; }
    }
}

