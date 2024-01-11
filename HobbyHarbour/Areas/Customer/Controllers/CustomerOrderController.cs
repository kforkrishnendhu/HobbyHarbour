using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HobbyHarbour.Data;
using HobbyHarbour.Models;
using HobbyHarbour.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Razorpay.Api;
//using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HobbyHarbour.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class CustomerOrderController : Controller
    {
        private readonly ILogger<CustomerHomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IWalletService _walletService;
        private readonly PdfGenerator _pdfGenerator;

        public CustomerOrderController(IWalletService walletService, ILogger<CustomerHomeController> logger, ApplicationDbContext db, PdfGenerator pdfGenerator)
        {
            _walletService = walletService;
            _logger = logger;
            _db = db;
            _pdfGenerator = pdfGenerator;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            // Retrieve order from the database
            var order = await _db.OrderItems.Include(o => o.Order) // Include the related Order data
                                            .Where(o => o.OrderItemID == id)
                                            .FirstOrDefaultAsync();

            // Credit the wallet with the order amount
            var credited = await _walletService.CreditWalletAsync(order.Order.UserID, order.TotalPrice, Method.OrderCancel);
            if (!credited)
            {
                return NotFound();
            }
            // Update order status to canceled
            order.OrderStatus = OrderStatus.Cancelled;

            // Save changes to the database
            await _db.SaveChangesAsync();

            return RedirectToAction("OrderDetails", "CustomerHome", new { itemId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnOrder(int id)
        {
            // Retrieve order from the database
            var order = await _db.OrderItems.Include(o => o.Order) // Include the related Order data
                                            .Where(o => o.OrderItemID == id)
                                            .FirstOrDefaultAsync();

            // Debit the wallet with the order amount
            var credited = await _walletService.CreditWalletAsync(order.Order.UserID, order.TotalPrice,Method.OrderReturn);
            if(!credited)
            {
                return NotFound();
            }
            // Update order status to returned
            order.OrderStatus = OrderStatus.Returned;

            // Save changes to the database
            await _db.SaveChangesAsync();

            return RedirectToAction("Orders", "CustomerHome");
        }

        [HttpGet]
        public IActionResult DownloadInvoice(int orderId)
        {
            // Get the order details and other necessary data
            var order = _db.Orders
                        .Include(o => o.LineItems)
                        .ThenInclude(o=>o.Product)
                        .Where(o => o.OrderID == orderId)
                        .FirstOrDefault();

            // Generate invoice number and date
            string invoiceNumber = GenerateInvoiceNumber();
            DateTime invoiceDate = DateTime.UtcNow; 

            order.InvoiceNumber = invoiceNumber;
            order.InvoiceDate = invoiceDate;

            _db.SaveChanges();

            // Generate and download the PDF
            var pdfBytes = _pdfGenerator.GenerateInvoice(order);

            // Return the PDF as a file
            return File(pdfBytes, "application/pdf", $"Invoice_{orderId}.pdf");

        }

        private string GenerateInvoiceNumber()
        {
              return $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }
}

