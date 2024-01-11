using System;
using HobbyHarbour.Data;
using HobbyHarbour.Models;
using Newtonsoft.Json.Linq;

namespace HobbyHarbour.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _db;

        public PaymentService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext db)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }
        public Task<MerchantOrder> ProcessMerchantOrder(Order payRequest)
        {
            try
            {
                _db.Entry(payRequest).Reference(o => o.User).Load(); //Include User information on payRequest Order Model.

                // Generate random receipt number for order
                Random randomObj = new Random();
                string transactionId = randomObj.Next(10000000, 100000000).ToString();
                Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient("rzp_test_GrmSxC8oqd7YMt", "Ld6BuJ4C8HEQeo4Y7L5lvKeB");
                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", payRequest.TotalAmount * 100);
                options.Add("receipt", transactionId);
                options.Add("currency", "INR");
                options.Add("payment_capture", "0"); // 1 - automatic  , 0 - manual
                //options.Add("Notes", "Test Payment of Merchant");
                Razorpay.Api.Order orderResponse = client.Order.Create(options);
                //string orderId = orderResponse["id"].ToString();

                //Console.WriteLine("Order Response Attributes:");
                //foreach (var attribute in orderResponse.Attributes)
                //{
                //    Console.WriteLine($"{attribute.Key}: {attribute.Value}");
                //}
                JProperty idProperty = orderResponse.Attributes.Property("id");
                string orderId = idProperty != null ? idProperty.Value.ToString() : null;

                MerchantOrder order = new MerchantOrder
                {
                    RazorpayOrderId=orderId,
                    OrderId = payRequest.OrderID.ToString(),
                    TransactionId = transactionId,
                    RazorpayKey = "rzp_test_GrmSxC8oqd7YMt",
                    Amount = payRequest.TotalAmount * 100,
                    Currency = "INR",
                    Name =payRequest.User.FirstName + " " + payRequest.User.LastName,
                    Email = payRequest.User.Email,
                    PhoneNumber = payRequest.User.PhoneNumber,
                    Address = "Test Address",
                    Description = "Order by Merchant"
                };
                return Task.FromResult(order);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<string> CompleteOrderProcess(IHttpContextAccessor _httpContextAccessor)
        {
            try
            {
                string paymentId = _httpContextAccessor.HttpContext.Request.Form["rzp_paymentid"];
                // This is orderId
                string orderId = _httpContextAccessor.HttpContext.Request.Form["rzp_orderid"];
                Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient("rzp_test_GrmSxC8oqd7YMt", "Ld6BuJ4C8HEQeo4Y7L5lvKeB");
                Razorpay.Api.Payment payment = client.Payment.Fetch(paymentId);
                // This code is for capture the payment 
                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", payment.Attributes["amount"]);
                Razorpay.Api.Payment paymentCaptured = payment.Capture(options);
                string amt = paymentCaptured.Attributes["amount"];
                return paymentCaptured.Attributes["status"];
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

