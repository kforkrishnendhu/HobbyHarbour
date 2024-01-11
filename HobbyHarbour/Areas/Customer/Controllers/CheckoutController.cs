using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HobbyHarbour.Data;
using HobbyHarbour.Models;
using HobbyHarbour.Service;
using HobbyHarbour.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Razorpay.Api;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HobbyHarbour.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class CheckoutController : Controller
    {
        private readonly ILogger<CheckoutController> _logger;
        private readonly IPaymentService _service;
        private IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _db;

        public CheckoutController(ApplicationDbContext db, ILogger<CheckoutController> logger, IPaymentService service, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _logger = logger;
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            var deliveryAddressModel = GetDeliveryAddressModel(); 
            var orderSummaryModel = GetOrderSummaryModel();
            var address = new Address();

            // Create a parent view model
            var checkoutViewModel = new CheckoutViewModel
            {
               DeliveryAddressModel = deliveryAddressModel,
                OrderSummaryModel = orderSummaryModel,
                AddAddressModel=address
            };

            return View(checkoutViewModel);
        }

        public List<Address> GetDeliveryAddressModel()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the user's ID
            var addresses = _db.Addresses.Where(a => a.UserID == userId).ToList();

            return addresses;
        }

        public List<Cart> GetOrderSummaryModel()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the user's ID

            var cartItems = _db.Cart
                                    .Include(c => c.Product)
                                    .ThenInclude(p => p.Category) // Include the product's category
                                    .Include(c=>c.Product.Images)
                                    .Where(c => c.UserID == userId)
                                    .ToList();

            return cartItems;
        }

        public IActionResult AddAddress()
        {
            return PartialView("_AddAddress");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAddress(Address model)
        {
            // Get the user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Assign the user's ID to the new address
            model.UserID = userId;

            // Model is valid; create and save the address
            _db.Addresses.Add(model);
            _db.SaveChanges();

            var deliveryAddressModel = GetDeliveryAddressModel();
            var orderSummaryModel = GetOrderSummaryModel();
            var address = new Address();

            // Create a parent view model
            var checkoutViewModel = new CheckoutViewModel
            {
                DeliveryAddressModel = deliveryAddressModel,
                OrderSummaryModel = orderSummaryModel,
                AddAddressModel = address
            };

            return View("Checkout",checkoutViewModel);

        }

        public IActionResult DeliveryAddressPartial()
        {
            // Get the user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Retrieve the list of addresses and pass it to the view
            var addresses = _db.Addresses.Where(a => a.UserID == userId).ToList();
            return PartialView("_DeliveryAddress", addresses);
        }


        public IActionResult EditAddress(int id)
        {
            // Fetch the address to be edited from the database
            var address = _db.Addresses.FirstOrDefault(a => a.AddressID == id);
            if (address == null)
            {
                // Handle the case where the address is not found
                return NotFound();
            }

            return PartialView("_EditAddress", address);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveEditedAddress(Address model)
        {
            if (ModelState.IsValid)
            {
                // Fetch the address to be updated from the database
                var addressToUpdate = _db.Addresses.FirstOrDefault(a => a.AddressID == model.AddressID);

                if (addressToUpdate == null)
                {
                    // Handle the case where the address is not found
                    return NotFound();
                }
                // Get the user's ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Update the address properties with the edited values
                addressToUpdate.UserID = userId;
                addressToUpdate.FullName = model.FullName;
                addressToUpdate.StreetAddress = model.StreetAddress;
                addressToUpdate.City = model.City;
                addressToUpdate.State = model.State;
                addressToUpdate.PostalCode = model.PostalCode;
                addressToUpdate.Country = model.Country;
                addressToUpdate.PhoneNumber = model.PhoneNumber;

                // Save the changes to the database
                _db.SaveChanges();

                var addresses = _db.Addresses.Where(a => a.UserID == userId).ToList();

                return PartialView("_DeliveryAddress", addresses);
            }

            // If ModelState is not valid, return the view with validation errors
            return PartialView("_EditAddress", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrderAsync(string sAddressId, string paymentMethod,string couponCode)
        {
            if (string.IsNullOrEmpty(sAddressId) || string.IsNullOrEmpty(paymentMethod))
            {
                return View("Checkout");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var selectedAddress = _db.Addresses.FirstOrDefault(a => a.AddressID == Convert.ToInt32(sAddressId) && a.UserID == userId);

            if (selectedAddress == null)
            {
                return View("Checkout");
            }
            string shippingAddress = selectedAddress.FullName + ", " + selectedAddress.StreetAddress + ", "
                                    + selectedAddress.City + ", " + selectedAddress.State + ", " + selectedAddress.Country
                                    + ", " + selectedAddress.PostalCode;

            var totalAmount = CalculateTotalAmount();
            totalAmount = totalAmount + 10; //add delivery charge default

            var order = new HobbyHarbour.Models.Order
            {
                UserID = userId,
                OrderDate = DateTime.Now,
                ShippingAddress = shippingAddress,
                TotalAmount = totalAmount
            };

            _db.Orders.Add(order);

            // Validate and apply the coupon
            if (!string.IsNullOrWhiteSpace(couponCode))
            {
                var coupon = _db.Coupons.FirstOrDefault(c => c.Code == couponCode);
                if (coupon != null && coupon.IsActive && coupon.ExpiryDate >= DateTime.Now)
                {
                    // Check if the coupon is already applied to the order
                    if (order.CouponApplied)
                    {
                        ModelState.AddModelError("CouponCode", "Coupon code already applied to this order.");
                        return View("Checkout");
                    }

                    // Check if the coupon is previously applied by the same user
                    var userAppliedCoupon = _db.Orders
                        .Where(o => o.UserID == userId && o.CouponCode == coupon.Code)
                        .FirstOrDefault();

                    if (userAppliedCoupon != null)
                    {
                        ModelState.AddModelError("CouponCode", "Coupon code already applied by the user.");
                        return View("Checkout");
                    }

                    var discount = CalculateCouponDiscount(totalAmount, coupon);
                    totalAmount -= discount;

                    coupon.UsageCount++; 
                    if (coupon.UsageCount >= coupon.UsageLimit)
                    {
                        coupon.IsActive = false; 
                    }

                    order.CouponApplied = true;
                    order.CouponCode = coupon.Code;
                    order.CouponDiscount = CalculateCouponDiscount(order.TotalAmount, coupon);
                    order.TotalAmount = totalAmount;

                    _db.SaveChanges();
                }
                else
                {
                    //ModelState.AddModelError("CouponCode", "Invalid or expired coupon code");
                    //return View("Checkout");
                    return Json(new { success = false, error = "Invalid or expired coupon code" });
                }
            }

            _db.SaveChanges();


            var cartItems = GetCartItems();  
            
            foreach (var Item in cartItems)
            {
                var orderItems = new OrderItem();
                orderItems.OrderID = order.OrderID; // Set the order ID for each order item
                orderItems.ProductID = Item.ProductID;
                orderItems.Quantity = Item.Quantity;
                orderItems.UnitPrice = Item.Product.Price;
                orderItems.TotalPrice = (Item.Product.Price) * (Item.Quantity);
                orderItems.OrderStatus = OrderStatus.Pending;
                _db.OrderItems.Add(orderItems);
            }
            _db.SaveChanges();

            // Handle payment based on the selected payment method
            if (paymentMethod== "cashOnDelivery")
            {
                var payment = new HobbyHarbour.Models.Payment
                {
                    OrderID = order.OrderID,
                    Amount = totalAmount,
                    PaymentMethod = "COD",
                    TransactionID=" ",
                    Status="Pending",
                    Date=DateTime.Now
                
                };
                _db.Add(payment);
                _db.SaveChanges();

                UpdateOrderStatusAndStock(order.OrderID); //Update the orderstatus to 'Confirmed'

                ClearUserCart(userId);     //clear user cart

                return Json(new { success = true, redirectUrl = Url.Action("CODOrderConfirmation", new { orderId = order.OrderID }) });
            }
            else if(paymentMethod == "onlinePayment")
            {
                MerchantOrder _marchantOrder = await _service.ProcessMerchantOrder(order);
                TempData["MerchantOrderData"] = JsonConvert.SerializeObject(_marchantOrder);
                return Json(new { success = true, redirectUrl = Url.Action("Payment") });

            }
            else
            {
                return RedirectToAction("OrderConfirmation", new { orderId = order.OrderID });
            }

        }

        private decimal CalculateCouponDiscount(decimal totalAmount, Coupon coupon)
        {
            if (coupon.DiscountType == DiscountType.Percentage)
            {
                return (coupon.DiscountValue / 100) * totalAmount;
            }
            else if (coupon.DiscountType == DiscountType.FixedAmount)
            {
                return Math.Min(coupon.DiscountValue, totalAmount);
            }

            return 0; 
        }

        public IActionResult Payment()
        {
            var merchantOrderDataJson = TempData["MerchantOrderData"] as string;

            // If TempData is not available, check URL parameters
            if (string.IsNullOrEmpty(merchantOrderDataJson))
            {
                merchantOrderDataJson = HttpContext.Request.Query["merchantOrderData"];
            }
            ViewBag.MerchantOrderData = merchantOrderDataJson;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CompleteOrderProcess([FromForm] string rzp_paymentid, [FromForm] string rzp_orderid, [FromForm] string MerchantOrderData)
        {
            // Deserialize the JSON string to MerchantOrder
            MerchantOrder model = JsonConvert.DeserializeObject<MerchantOrder>(MerchantOrderData);

            string PaymentMessage = await _service.CompleteOrderProcess(_httpContextAccessor);
            if (PaymentMessage == "captured")
            {
                if(rzp_orderid!=null)
                {
                    UpdateOrderStatusAndStock(Convert.ToInt32(model.OrderId)); //Update the orderstatus to 'Confirmed'

                    var payment = new HobbyHarbour.Models.Payment          //Update Payment table
                    {
                        OrderID = Convert.ToInt32(model.OrderId),
                        Amount = model.Amount,
                        PaymentMethod = "Online Payment",
                        TransactionID = model.TransactionId,
                        Status = "Paid",
                        Date = DateTime.Now

                    };
                    _db.Add(payment);
                    _db.SaveChanges();
                }
                // Retrieve user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ClearUserCart(userId);     //clear user cart
                return RedirectToAction("Success",model);
            }
            else
            {
                return RedirectToAction("Failed");
            }
        }
        public IActionResult Success(MerchantOrder model)
        {
            return View(model);
        }
        public IActionResult Failed()
        {
            return View();
        }

        private decimal CalculateTotalAmount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the user's ID

            var cartItems = _db.Cart
                                    .Include(c => c.Product)
                                    .ThenInclude(p => p.Category) // Include the product's category
                                    .Where(c => c.UserID == userId)
                                    .ToList();
            decimal total=0;
            foreach(var item in cartItems)
            {
                total = total + (item.Product.Price) * (item.Quantity);
            }

            return total;
        }

        private List<Cart> GetCartItems()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the user's ID

            var cartItems = _db.Cart
                                    .Include(c => c.Product)
                                    .ThenInclude(p => p.Category) // Include the product's category
                                    .Where(c => c.UserID == userId)
                                    .ToList();
            return cartItems;
        }

        public void UpdateOrderStatusAndStock(int orderId)
        {
            var orderItemsToUpdate = _db.OrderItems
                .Where(item => item.OrderID == orderId)
                .ToList();

            foreach (var orderItem in orderItemsToUpdate)
            {
                orderItem.OrderStatus = OrderStatus.Confirmed;

                // Assuming there's a ProductId property in your OrderItem entity
                var product = _db.Products.Find(orderItem.ProductID);

                if (product != null)
                {
                    // Adjust the stock quantity based on the ordered quantity
                    product.StockQuantity -= orderItem.Quantity;
                }
            }

            _db.SaveChanges();
        }

        private void ClearUserCart(string userId)
        {
            // Retrieve the cart items for the user
            var cartItems = _db.Cart.Where(c => c.UserID == userId).ToList();

            // Remove the cart items from the database
            _db.Cart.RemoveRange(cartItems);

            // Save changes to the database
            _db.SaveChanges();
        }

        public IActionResult CODOrderConfirmation(int orderId)
        {
            // Retrieve order details from the database based on the orderId
            var order = _db.Orders.Include(c=>c.User).FirstOrDefault(o => o.OrderID == orderId);

            if (order == null)
            {
                return RedirectToAction("Error");
            }

            // Pass the order details to the view
            return View(order);
        }


    }

   

}


