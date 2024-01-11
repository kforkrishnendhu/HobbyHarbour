using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HobbyHarbour.Models;
using HobbyHarbour.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.CodeAnalysis;
using HobbyHarbour.ViewModels;
using QuestPDF.Helpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Drawing.Printing;
using QuestPDF.Infrastructure;

namespace HobbyHarbour.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize(Roles = "Customer")]
public class CustomerHomeController : Controller
{
    private readonly ILogger<CustomerHomeController> _logger;
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CustomerHomeController(ILogger<CustomerHomeController> logger, ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _logger = logger;
        _db = db;                                           //is registered as a service in the program.cs file. Now we can
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index(int page = 1, int pageSize = 8)
    {
        List<Product> objProductList = _db.Products.Include(p => p.Category).Include(p => p.Images).Where(p=>!p.IsDeleted).OrderByDescending(p=>p.ProductID).ToList();
        // Determine the total number of products for pagination
        var totalProducts = objProductList.Count();
        objProductList= _db.Products.Include(p => p.Category).Include(p => p.Images).Where(p => !p.IsDeleted).OrderByDescending(p => p.ProductID).Skip((page - 1) * pageSize).Take(pageSize).ToList();


        // Pass the paginated products and pagination information to the view
        var viewModel = new ProductIndexViewModel
        {
            Products = objProductList,
            PageInfo = new PageInfo { CurrentPage = page, ItemsPerPage = pageSize, TotalItems = totalProducts }
        };

        return View(viewModel);
    }

    public IActionResult BoardGames(int page = 1, int pageSize = 8)
    {
        List<Product> objProductList = _db.Products
       .Include(p => p.Category)
       .Include(p => p.Images)
       .Where(p => !p.IsDeleted && p.Category.CategoryName == "Board Games")
       .OrderByDescending(p => p.ProductID)
       .Skip((page - 1) * pageSize)
       .Take(pageSize)
       .ToList();

        // Determine the total number of board games for pagination
        var totalBoardGames = _db.Products
            .Count(p => !p.IsDeleted && p.Category.CategoryName == "Board Games");

        // Pass the paginated board games and pagination information to the view
        var viewModel = new ProductIndexViewModel
        {
            Products = objProductList,
            PageInfo = new PageInfo { CurrentPage = page, ItemsPerPage = pageSize, TotalItems = totalBoardGames }
        };

        return View("Index", viewModel);
    }

    public IActionResult OutdoorToys(int page = 1, int pageSize = 8)
    {
        List<Product> objProductList = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => !p.IsDeleted && p.Category.CategoryName == "Outdoor Toys")
            .OrderByDescending(p => p.ProductID)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Determine the total number of outdoor toys for pagination
        var totalOutdoorToys = _db.Products
            .Count(p => !p.IsDeleted && p.Category.CategoryName == "Outdoor Toys");

        // Pass the paginated outdoor toys and pagination information to the view
        var viewModel = new ProductIndexViewModel
        {
            Products = objProductList,
            PageInfo = new PageInfo { CurrentPage = page, ItemsPerPage = pageSize, TotalItems = totalOutdoorToys }
        };

        return View("Index", viewModel);
    }


    public IActionResult RemoteToys(int page = 1, int pageSize = 8)
    {
        List<Product> objProductList = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => !p.IsDeleted && p.Category.CategoryName == "Remote Controls")
            .OrderByDescending(p => p.ProductID)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Determine the total number of remote toys for pagination
        var totalRemoteToys = _db.Products
            .Count(p => !p.IsDeleted && p.Category.CategoryName == "Remote Controls");

        // Pass the paginated remote toys and pagination information to the view
        var viewModel = new ProductIndexViewModel
        {
            Products = objProductList,
            PageInfo = new PageInfo { CurrentPage = page, ItemsPerPage = pageSize, TotalItems = totalRemoteToys }
        };

        return View("Index", viewModel);
    }


    public IActionResult SoftToys(int page = 1, int pageSize = 8)
    {
        List<Product> objProductList = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => !p.IsDeleted && p.Category.CategoryName == "Soft Toys")
            .OrderByDescending(p => p.ProductID)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Determine the total number of soft toys for pagination
        var totalSoftToys = _db.Products
            .Count(p => !p.IsDeleted && p.Category.CategoryName == "Soft Toys");

        // Pass the paginated soft toys and pagination information to the view
        var viewModel = new ProductIndexViewModel
        {
            Products = objProductList,
            PageInfo = new PageInfo { CurrentPage = page, ItemsPerPage = pageSize, TotalItems = totalSoftToys }
        };

        return View("Index", viewModel);
    }


    public IActionResult Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the user's ID

        var cartItem = _db.Cart
       .FirstOrDefault(c => c.ProductID == id && c.UserID == userId);

        if( cartItem != null)
        {
            TempData["ItemAddedToCart"] = true;
        }
        else
        {
            TempData["ItemAddedToCart"] = null;
        }

        var product = _db.Products.Include(p => p.Category).Include(p=>p.Images).Where(p=>!p.IsDeleted)
                        .FirstOrDefault(p => p.ProductID == id);
        if (product != null)
        {
            return View(product);
        }
        else
        {
            return NotFound();
        }
    }

    [HttpPost] // This ensures the user is authenticated before adding to the cart
    public IActionResult AddToCart(int productId, int quantity, string action)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the user's ID

            if (action == "AddToCart")
            {
                var product = _db.Products.Find(productId);

                if (product == null)
                {
                    // Handle the case where the product is not found
                    return RedirectToAction("Error");
                }

                if (product.StockQuantity < quantity)
                {
                    // Insufficient stock, redirect to a page indicating unavailability
                    TempData["InsufficientStock"] = true;
                    return RedirectToAction("Details", new { id = productId });
                }

                // Check if the item is already in the cart
                var existingCartItem = _db.Cart.SingleOrDefault(c => c.UserID == userId && c.ProductID == productId);

                if (existingCartItem != null)
                {
                    // Update the quantity
                    existingCartItem.Quantity += quantity;
                }
                else
                {
                    // Create a new cart item
                    var cartItem = new Cart
                    {
                        UserID = userId,
                        ProductID = productId,
                        Quantity = quantity
                    };

                    _db.Cart.Add(cartItem);
                }

                _db.SaveChanges(); // Save changes to the database

                // Set a flag or variable to indicate an item has been added to the cart
                TempData["ItemAddedToCart"] = true;
            }
            else if (action == "GoToCart")
            {
                return RedirectToAction("Cart");
            }

            return RedirectToAction("Details", new { id = productId });
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error"); // Redirect to an error page
        }
    }

    public IActionResult Cart()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the user's ID

        var cartItems = _db.Cart
                        .Include(c => c.Product)
                        .ThenInclude(p => p.Category) // Include the product's category
                        .Include(c => c.Product.Images)  // Include the product's images
                        .Where(c => c.UserID == userId)
                        .ToList();

        return View(cartItems);
    }

    public IActionResult Wishlist()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the user's ID

        var items = _db.WishlistItems
                                .Include(c => c.Product)
                                .ThenInclude(c => c.Category) // Include the product's category
                                .Include(c=>c.Product.Images)
                                .Where(c => c.UserID == userId)
                                .ToList();

        return View(items);
    }

    public IActionResult RemoveFromWishlist(int productId)
    {
        // Get the user's ID
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Find the cart item for the product to remove
        var wishlistItem = _db.WishlistItems.SingleOrDefault(c => c.UserID == userId && c.ProductID == productId);

        if (wishlistItem != null)
        {
            // Remove the cart item
            _db.WishlistItems.Remove(wishlistItem);
            _db.SaveChanges();
        }

        var items = _db.WishlistItems
                                .Include(c => c.Product)
                                .ThenInclude(c => c.Category) // Include the product's category
                                .Include(c => c.Product.Images)
                                .Where(c => c.UserID == userId)
                                .ToList();

        return View("Wishlist",items);
    }

    [HttpPost]
    public IActionResult UpdateCartQuantity(int productId, int quantity)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the user's ID

        var cartItem = _db.Cart.SingleOrDefault(c => c.UserID == userId && c.ProductID == productId);

        if (cartItem != null)
        {
            var product = _db.Products.Find(productId);

            if (product == null)
            {
                // Handle the case where the product is not found
                return RedirectToAction("Error");
            }
            // Check if the requested quantity is greater than the available stock
            if (product.StockQuantity < quantity)
            {
                var cartItemsWithCategories = _db.Cart
                    .Include(c => c.Product)
                    .ThenInclude(c => c.Category)
                    .Include(c=>c.Product.Images)
                    .Where(c => c.UserID == userId)
                    .ToList();

                // Return the view with the cart items and an error message
                TempData["InsufficientStock"] = true;
                TempData["InsufficientStockProductID"] = product.ProductID;
                return PartialView("_CartItems", cartItemsWithCategories);
            }

            cartItem.Quantity = quantity;
            _db.SaveChanges();
        }

        var cartItems = _db.Cart
                                .Include(p => p.Product)
                                .ThenInclude(p => p.Category) // Include the product's category
                                .Include(p=>p.Product.Images)
                                .Where(p => p.UserID == userId)
                                .ToList();
        return PartialView("_CartItems", cartItems);
    }


    public IActionResult RemoveFromCart(int productId)
    {
        // Get the user's ID
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Find the cart item for the product to remove
        var cartItem = _db.Cart.SingleOrDefault(c => c.UserID == userId && c.ProductID == productId);

        if (cartItem != null)
        {
            // Remove the cart item
            _db.Cart.Remove(cartItem);
            _db.SaveChanges();
        }

        return RedirectToAction("Cart"); // Redirect to the Cart page after removing the item
    }


    public async Task<IActionResult> AddToWishlist(int productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the user's ID

        var existingCartItem = await _db.Cart.SingleOrDefaultAsync(
            c => c.UserID == userId && c.ProductID == productId);

        if (existingCartItem != null)
        {
            // If the item is in the cart, remove it from the cart
            _db.Cart.Remove(existingCartItem);
            await _db.SaveChangesAsync();
        }

        var existingWishlistItem = await _db.WishlistItems.SingleOrDefaultAsync(
            wi => wi.UserID == userId && wi.ProductID == productId);

        if (existingWishlistItem == null)
        {
            var wishlistItem = new WishlistItem
            {
                UserID = userId,
                ProductID = productId,
                AddedDateTime = DateTime.Now
            };

            _db.WishlistItems.Add(wishlistItem);
            await _db.SaveChangesAsync();

            TempData["ItemAddedToWishlist"] = true;
        }

        return RedirectToAction("Cart"); 
    }


    public IActionResult Orders(int page = 1, int pageSize = 8)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Retrieve the latest order ID for the current user
        var orderIdList = _db.Orders
            .Include(o=>o.LineItems)
            .Where(o => o.UserID == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        var totalProducts = orderIdList.Count();

        orderIdList = _db.Orders
           .Include(o => o.LineItems)
           .Where(o => o.UserID == userId)
           .OrderByDescending(o => o.OrderDate)
           .Skip((page - 1) * pageSize)
           .Take(pageSize)
           .ToList();
        var viewModel = new OrderIndexViewModel
        {
            Orders = orderIdList,
            PageInfo = new PageInfo { CurrentPage = page, ItemsPerPage = pageSize, TotalItems = totalProducts }
        };

        if (orderIdList == null)
        {
            // Handle the case where the order with the given ID is not found
            return RedirectToAction("Index", "CustomerHome"); // Redirect to the home page or another appropriate action
        }

        return View(viewModel);
    }

    public IActionResult OrderItems(int orderId)
    {
        var orders = _db.OrderItems
                    .Include(oi => oi.Order)
                    .Include(oi => oi.Product)
                    .ThenInclude(p => p.Images) // Assuming Images is a navigation property in Product
                    .Where(oi => oi.OrderID == orderId && oi.OrderStatus != OrderStatus.Pending)
                    .OrderByDescending(oi => oi.OrderID)
                    .ToList();

        if (orders == null)
        {
            // Handle the case where the order with the given ID is not found
            return RedirectToAction("Index", "CustomerHome"); // Redirect to the home page or another appropriate action
        }

        return View(orders);
    }

    public IActionResult OrderDetails(int itemId)
    {
        if (itemId == null)
        {
            // Handle the case where no item IDs are provided
            return RedirectToAction("Orders", "CustomerHome"); // Redirect to the home page or another appropriate action
        }
        var order = _db.OrderItems
            .Include(o => o.Order)
                .Include(oi => oi.Product)
                .ThenInclude(oi=>oi.Images)
            .Where(o => o.OrderItemID == itemId).FirstOrDefault();

        if (order == null)
        {
            // Handle the case where the order with the given ID is not found
            return RedirectToAction("Orders", "CustomerHome"); // Redirect to the home page or another appropriate action
        }

        return View(order);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

