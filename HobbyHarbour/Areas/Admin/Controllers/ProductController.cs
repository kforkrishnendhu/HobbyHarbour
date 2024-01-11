using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using HobbyHarbour.Data;
using HobbyHarbour.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HobbyHarbour.Areas.Admin.Controllers
{
    [Area("Admin")] // Specify the area
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)    //Here it is actually doing the dependancy injection inside  
        {                                                     //the constructor for using the application db context that 
            _db = db;                                           //is registered as a service in the program.cs file. Now we can
            _webHostEnvironment = webHostEnvironment;           //use the database tables using this _db object.
        }                                                     

        // GET: /<controller>/
        public IActionResult Index()
        {
            //List<Product> objProductList = _db.Products.ToList();
            List<Product> objProductList = _db.Products.Include(p => p.Category).ToList();

            return View(objProductList);
        }

        public IActionResult Create()
        {
           
            var categories = _db.Categories.Where(c => !c.IsDeleted).ToList();

            // Create a SelectList of categories to use with the dropdown
            ViewBag.CategoryList = new SelectList(categories, "CategoryID", "CategoryName");
            var model = new Product
            {
                Images = new List<ProductImage>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public IActionResult Create(Product model, List<IFormFile>? Images)
        {
            if (ModelState.IsValid)
            {
                // Process and save images
                foreach (var image in Images)
                {
                    model.Images.Add(new ProductImage { ImageUrl = ProcessAndSaveImage(image) });
                }

                // Save the product to the database
                _db.Products.Add(model);
                _db.SaveChanges();

                return RedirectToAction("Index");
            }

            // If the model state is not valid, populate the category dropdown again
            ViewBag.CategoryList = new SelectList(_db.Categories.Where(c => !c.IsDeleted).ToList(), "CategoryID", "CategoryName");

            return View(model);
        }

        private string ProcessAndSaveImage(IFormFile image)
        {
            if (image != null)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images/product");

                using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    image.CopyTo(fileStream);
                }

                return @"/images/product/" + fileName;
            }

            return string.Empty;
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var categories = _db.Categories.Where(c => !c.IsDeleted).ToList();

            ViewBag.CategoryList = new SelectList(categories, "CategoryID", "CategoryName");

            var product = _db.Products
                .Include(p => p.Images) // Include related images
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            var editModel = new Product
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryID = product.CategoryID,
                Images = product.Images.ToList() // Copy the list of images
            };

            return View(editModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product editModel, List<IFormFile> Images)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                // Process and save each uploaded image
                foreach (var image in Images)
                {
                    if (image != null)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        string productPath = Path.Combine(wwwRootPath, @"images/product");

                        using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                        {
                            image.CopyTo(fileStream);
                        }

                        // Add the new image to the list
                        editModel.Images.Add(new ProductImage { ImageUrl = @"/images/product/" + fileName });
                    }
                }

                // Delete old images that are not in the updated list
                var oldImagesToDelete = editModel.ImagesToDelete ?? new List<int>();
                foreach (var imageId in oldImagesToDelete)
                {
                    var oldImage = _db.ProductImages.Find(imageId);
                    if (oldImage != null)
                    {
                        // Delete the old image file
                        var oldImagePath = Path.Combine(wwwRootPath, oldImage.ImageUrl.TrimStart('/', '\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }

                        // Remove the old image from the list
                        _db.ProductImages.Remove(oldImage);
                    }
                }


                _db.Products.Update(editModel);
                _db.SaveChanges();

                return RedirectToAction("Index"); // Redirect to the product list after a successful edit
            }

            // Model is not valid; redisplay the Edit view with validation errors
            return View(editModel);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            // Retrieve the category with the specified ID from the database
            var product = _db.Products.Find(id);

            if (product == null)
            {
                // Category not found, return a not found view or redirect
                return NotFound();
            }

            return View(product); // Display the Delete view with the category data
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Use this attribute to prevent cross-site request forgery (CSRF) attacks
        public IActionResult DeleteConfirmed(int id)
        {
            // Find the category in the database by its ID
            var product = _db.Products
                            .Include(p => p.Images) // Include related images
                            .FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
               return NotFound();
            }


            //delete the old image
            foreach (var image in product.Images)
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            product.IsDeleted = true;
            _db.SaveChanges();

            return RedirectToAction("Index"); // Redirect to the category list after successful deletion
        }

        public IActionResult Activate(int id)
        {
            // Find the category in the database by its ID
            var product = _db.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            product.IsDeleted = false;
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}

