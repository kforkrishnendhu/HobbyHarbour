using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using HobbyHarbour.Data;
using HobbyHarbour.Models;
using HobbyHarbour.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HobbyHarbour.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProductsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(int? selectedCategory, string? priceRange, int page = 1, int pageSize = 9, string search=null)
        {
            var categories = _db.Categories.Where(c => !c.IsDeleted).ToList();

            // Create a SelectList of categories to use with the dropdown
            ViewBag.CategoryList = new SelectList(categories, "CategoryID", "CategoryName");

            IQueryable<Product> query = _db.Products.Include(p => p.Category).Include(p => p.Images).Where(p => !p.IsDeleted).OrderByDescending(p=>p.ProductID);

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.ProductName.Contains(search) || p.Category.CategoryName.Contains(search));
            }

            // Apply category filter
            if (selectedCategory.HasValue)
            {
                query = query.Where(p => p.CategoryID == selectedCategory);
            }

            // Apply price range filter
            if (!string.IsNullOrEmpty(priceRange))
            {
                var priceRanges = priceRange.Split('-');
                if (priceRanges.Length == 2 && decimal.TryParse(priceRanges[0], out decimal minPrice) && decimal.TryParse(priceRanges[1], out decimal maxPrice))
                {
                    query = query.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
                }
                if (priceRange == "150+")
                {
                    // Include products with a price greater than or equal to $150
                    query = query.Where(p => p.Price >= 150);
                }
            }
            // Determine the total number of products for pagination
            var totalProducts = query.Count();
            var paginatedProducts = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

           

            // Pass the paginated products and pagination information to the view
            var viewModel = new ProductIndexViewModel
            {
                Products = paginatedProducts,
                PageInfo = new PageInfo { CurrentPage = page, ItemsPerPage = pageSize, TotalItems = totalProducts }
            };

            ViewBag.selectedCategory = selectedCategory;
            ViewBag.priceRange = priceRange;
            ViewBag.search = search;

            return View(viewModel);
        }
    }
}

