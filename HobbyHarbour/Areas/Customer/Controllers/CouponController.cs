using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HobbyHarbour.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HobbyHarbour.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class CouponController : Controller
    {
        public readonly ApplicationDbContext _db;

        public CouponController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get the list of coupon codes that the user has applied before
            var usedCouponCodes = _db.Orders
                .Where(o => o.UserID == userId && o.CouponApplied)
                .Select(o => o.CouponCode)
                .ToList();

            // Get the active coupons excluding the ones the user has already used
            var activeCoupons = _db.Coupons
                .Where(c => c.IsActive && !usedCouponCodes.Contains(c.Code))
                .ToList();

            return View(activeCoupons);
        }
    }
}

