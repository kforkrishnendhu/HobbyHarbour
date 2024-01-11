using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HobbyHarbour.Models
{
    public class WishlistItem
    {
        [Key]
        public int WishlistItemId { get; set; }

        [Required]
        public string UserID { get; set; } // The ID of the user who added the item to the wishlist

        [Required]
        public int ProductID { get; set; } // The ID of the product in the wishlist

        public DateTime AddedDateTime { get; set; } // Timestamp when the item was added to the wishlist

        // Define the foreign key relationships
        [ForeignKey("UserID")]
        public ApplicationUser? User { get; set; }

        [ForeignKey("ProductID")]
        public Product? Product { get; set; }
    }

}

