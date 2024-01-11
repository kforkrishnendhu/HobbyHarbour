using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using HobbyHarbour.Data;
using HobbyHarbour.Models;
using HobbyHarbour.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HobbyHarbour.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class CustomerAccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public CustomerAccountController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                // User not found; handle this as needed (e.g., redirect to an error page)
                return NotFound();
            }

            // Create a model to represent the user's profile
            var userProfile = new UserProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
                
            };

            return View(userProfile);
        }


        public IActionResult EditProfile()
        {
            // Retrieve user profile data and populate the EditUserProfileViewModel
            var user = _userManager.GetUserAsync(User).Result;
            var editModel = new UserProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return View(editModel);
        }



        [HttpPost]
        public IActionResult SaveProfileField(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, return the edit view with validation errors
                return View("EditProfile", model);
            }
            // Retrieve user data and update properties
            var user = _userManager.GetUserAsync(User).Result;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            // Update user profile in the database
            var updateResult = _userManager.UpdateAsync(user).Result;

            if (updateResult.Succeeded)
            {
                // Redirect to the user profile page after successful update
                return RedirectToAction("Profile");
            }

            // Handle errors if the update is not successful
            ModelState.AddModelError(string.Empty, "Error updating user profile");
            return View("EditProfile", model);
        }

        public IActionResult Addresses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the user's ID
            var addresses = _db.Addresses.Where(a => a.UserID == userId).ToList();

            return View(addresses);
        }

        public IActionResult AddAddress()
        {
            return View();
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

                return RedirectToAction("Addresses");
          
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

            return View(address);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAddress(Address model)
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

                return RedirectToAction("Addresses");
            }

            // If ModelState is not valid, return the view with validation errors
            return View("EditAddress", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAddress(int id)
        {
            // Fetch the address to be deleted from the database
            var address = _db.Addresses.FirstOrDefault(a => a.AddressID == id);
            if (address == null)
            {
                // Handle the case where the address is not found
                return NotFound();
            }

            // Remove the address and save changes
            _db.Addresses.Remove(address);
            _db.SaveChanges();

            return RedirectToAction("Addresses");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user != null)
                {
                    var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                    if (changePasswordResult.Succeeded)
                    {
                        // Password successfully changed
                        var success = true;

                        return Json(new { success });
                    }
                    else
                    {
                        foreach (var error in changePasswordResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Wallet()
        {
            ApplicationUser user = _userManager.GetUserAsync(User).Result;

            if (user == null)
            {
                return NotFound();
            }

            Wallet wallet = _db.Wallets
                                .Include(w=>w.User)
                                .Include(w => w.WalletHistories)
                                .FirstOrDefault(w => w.UserID == user.Id);
            if (wallet == null)
            {
                return NotFound();
            }

            return View(wallet);
        }

    }
}

