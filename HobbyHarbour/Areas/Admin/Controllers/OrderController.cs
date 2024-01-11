using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using HobbyHarbour.Data;
using HobbyHarbour.Models;
using HobbyHarbour.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HobbyHarbour.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWalletService _walletService;


        public OrderController(IWalletService walletService, ApplicationDbContext db)
        {
            _walletService = walletService;
            _db = db;
        }

        public IActionResult Index(OrderStatus? status, DateTime? startDate, DateTime? endDate)
        {
            IQueryable<OrderItem> query = _db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product);

            if (status.HasValue)
            {
                query = query.Where(oi => oi.OrderStatus == status);
            }

            if (startDate.HasValue)
            {
                query = query.Where(oi => oi.Order.OrderDate >= startDate);
            }

            if (endDate.HasValue)
            {
                query = query.Where(oi => oi.Order.OrderDate <= endDate);
            }

            List<OrderItem> orders = query.OrderByDescending(oi => oi.Order.OrderID).ToList();

            return View(orders);
        }

        // POST: /AdminOrder/Index
        [HttpPost]
        public IActionResult Index(string status, DateTime? startDate, DateTime? endDate)
        {
            return RedirectToAction("Index", new { status, startDate, endDate });
        }

        public IActionResult OrderDetails(int orderID)
        {
               var orderDetails = _db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .ThenInclude(oi => oi.Images)
                .Where(oi => oi.OrderID == orderID)
                .FirstOrDefault();

            return View(orderDetails);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderID, OrderStatus newStatus)
        {
            var order =await _db.OrderItems.Include(o => o.Order).Where(o=>o.OrderID==orderID).FirstOrDefaultAsync();
            if (order != null)
            {
                if(newStatus==OrderStatus.Cancelled)
                {
                    var credited = await _walletService.CreditWalletAsync(order.Order.UserID, order.TotalPrice, Method.OrderCancel);
                    if (!credited)
                    {
                        return NotFound();
                    }
                }
               
                order.OrderStatus = newStatus;
                await _db.SaveChangesAsync();

            }

            return RedirectToAction("OrderDetails", new { orderID = orderID });
        }


    }
}

