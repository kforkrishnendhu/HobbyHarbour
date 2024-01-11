using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HobbyHarbour.Models
{
    public class Order
    {
        public int OrderID { get; set; }

        public string UserID { get; set; }

        [ForeignKey("UserID")]
        public ApplicationUser? User { get; set; }

        public DateTime OrderDate { get; set; }

        [Required]
        [MaxLength(255)]
        public string ShippingAddress { get; set; }

        [Required]
        [Range(1, 1000)]
        public decimal TotalAmount { get; set; }

        public string? InvoiceNumber { get; set; } = "";

        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        // Collection of order items
        public ICollection<OrderItem> LineItems { get; set; } = new List<OrderItem>();

        public bool CouponApplied { get; set; } = false;

        public string? CouponCode { get; set; }

        public decimal CouponDiscount { get; set; } = 0;

    }

    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Shipped,
        Delivered,
        Cancelled,
        Returned
    }
}

