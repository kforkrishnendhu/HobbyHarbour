using System;
namespace HobbyHarbour.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Coupon
    {
        public int CouponID { get; set; }

        [Required(ErrorMessage = "The Code field is required.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "The Discount Type field is required.")]
        public DiscountType DiscountType { get; set; }

        [Required(ErrorMessage = "The Discount Value field is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Discount Value must be a non-negative number.")]
        public decimal DiscountValue { get; set; }

        [Required(ErrorMessage = "The Minimum Cart Amount field is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Minimum Cart Amount must be a non-negative number.")]
        public decimal MinimumCartAmount { get; set; }

        [Required(ErrorMessage = "The Maximum Cart Amount field is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Maximum Cart Amount must be a non-negative number.")]
        public decimal MaximumCartAmount { get; set; }

        [Required(ErrorMessage = "The Expiry Date field is required.")]
        public DateTime ExpiryDate { get; set; }

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "The Usage Limit field is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Usage Limit must be a non-negative integer.")]
        public int UsageLimit { get; set; }

        public int UsageCount { get; set; } = 0;
    }

    public enum DiscountType
    {
        Percentage,
        FixedAmount
    }


}

