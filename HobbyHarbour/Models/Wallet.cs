using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HobbyHarbour.Models
{
    public class Wallet
    {
        [Key]
        public int WalletID { get; set; }

        [Required]
        public string UserID { get; set; }

        [ForeignKey("UserID")]
        public ApplicationUser? User { get; set; }

        [DataType(DataType.Currency)]
        public decimal Balance { get; set; }

        public List<WalletHistory> WalletHistories { get; set; }
    }

    public class WalletHistory
    {
        [Key]
        public int WalletHistoryID { get; set; }

        [Required]
        public int WalletID { get; set; }

        [ForeignKey("WalletID")]
        public Wallet? Wallet { get; set; }

        public DateTime Date { get; set; }

        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        public Method Method { get; set; }
    }

    public enum Method
    {
        OrderReturn,
        OrderCancel,
        WalletPayment
    }

}

