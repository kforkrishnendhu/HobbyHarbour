using System;
using System.ComponentModel.DataAnnotations;
using HobbyHarbour.Models;

namespace HobbyHarbour.ViewModels
{
    public class CheckoutViewModel
    {
        public List<Address> DeliveryAddressModel { get; set; }
        public List<Cart> OrderSummaryModel { get; set; }
        public Address AddAddressModel { get; set; }

        [Display(Name = "Coupon Code")]
        public string CouponCode { get; set; }
    }

}

