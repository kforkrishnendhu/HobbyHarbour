using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HobbyHarbour.Models
{
	public class Payment
	{
        public int PaymentID { get; set; }

        public int OrderID { get; set; }

        [ForeignKey("OrderID")]
        public Order? Order { get; set; }

        [Required]
        [Range(1, 1000)]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; }

        [Required]
        public string TransactionID { get; set; }

        [Required]
        public string Status { get; set; }

        public DateTime Date { get; set; }

       
    }
}

