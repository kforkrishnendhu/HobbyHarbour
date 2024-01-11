using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using HobbyHarbour.Data;
using HobbyHarbour.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HobbyHarbour.Areas.Admin.Controllers
{

    [Area("Admin")] // Specify the area
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoryController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)    //Here it is actually doing the dependancy injection inside  
        {                                                     //the constructor for using the application db context that 
            _db = db;                                         //is registered as a service in the program.cs file. Now we can 
            _webHostEnvironment = webHostEnvironment;
        }                                                     //use the database tables using this _db object.

        // GET: /<controller>/
        public IActionResult Index()
        {
            List<Category> objCategoryList = _db.Categories.ToList();
            return View(objCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Use this attribute to prevent cross-site request forgery (CSRF) attacks
        public IActionResult Create(Category model)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate CategoryName
                bool isDuplicate = _db.Categories.Any(c => c.CategoryName == model.CategoryName);

                if (isDuplicate)
                {
                    ModelState.AddModelError("CategoryName", "Category with the same name already exists.");
                    return View(model);
                }

                _db.Categories.Add(model);
                _db.SaveChanges();


                return RedirectToAction("Index"); 
            }

            return View(model);
        }


        //private string UploadImage(IFormFile image)
        //{
        //    string wwwRootPath = _webHostEnvironment.WebRootPath;

        //    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
        //    string productPath = Path.Combine(wwwRootPath, @"images/product");

        //    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
        //    {
        //        image.CopyTo(fileStream);
        //    }

        //    return @"/images/category/" + fileName;
        //}


        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Retrieve the category with the specified ID from the database
            var category = _db.Categories.Find(id);

            if (category == null)
            {
                // Category not found, return a not found view or redirect
                return NotFound();
            }

            // Map the category data to a CategoryViewModel or edit model
            var editModel = new Category
            {
                CategoryID = category.CategoryID,
                CategoryName = category.CategoryName,
                Description = category.Description
                // Map other properties as needed
            };

            return View(editModel); // Display the Edit view with the edit model
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category editModel)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Update(editModel);
                _db.SaveChanges();

                return RedirectToAction("Index"); 
            }

            return View(editModel);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var category = _db.Categories.Find(id);

            if (category == null)
            {
                // Category not found, return a not found view or redirect
                return NotFound();
            }

            return View(category); // Display the Delete view with the category data
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Use this attribute to prevent cross-site request forgery (CSRF) attacks
        public IActionResult DeleteConfirmed(int id)
        {
            // Find the category in the database by its ID
            var category = _db.Categories.Find(id);

            if (category == null)
            {
                return NotFound();
            }

            // Reassign products and then soft delete the category
            ReassignProductsToUncategorized(category);

            //Soft Delete the category
            category.IsDeleted = true;
            _db.SaveChanges();

            return RedirectToAction("Index"); 
        }

        private void ReassignProductsToUncategorized(Category category)
        {
            // Retrieve all products associated with the category
            var productsToReassign = _db.Products.Where(p => p.CategoryID == category.CategoryID).ToList();

            // Get the "Uncategorized" category
            var uncategorizedCategory = _db.Categories.SingleOrDefault(c => c.CategoryName == "Uncategorized");

            if (uncategorizedCategory != null)
            {
                // Reassign products to the "Uncategorized" category
                foreach (var product in productsToReassign)
                {
                    product.CategoryID = uncategorizedCategory.CategoryID;
                }

                _db.SaveChanges();
            }
        }

         public IActionResult Activate(int id)
        {
            // Find the category in the database by its ID
            var category = _db.Categories.Find(id);

            if (category == null)
            {
                return NotFound();
            }

             category.IsDeleted = false;
            _db.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}

