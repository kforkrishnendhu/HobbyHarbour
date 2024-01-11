using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HobbyHarbour.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace HobbyHarbour.Areas.Home.Controllers;

[Area("Home")]
[Authorize(Roles = "Home")]
public class HomeController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost]
    public IActionResult OTPLogin(string phoneNumber)
    {
        // Set your Twilio Account SID and Auth Token
        string accountSid = "ACea13ca6a350436054915ffe7f76d49ef";
        string authToken = "24413f08bc9b7f7f798a924de160f520";
        TwilioClient.Init(accountSid, authToken);

        // Generate a random OTP (for example, a 6-digit code)
        string otp = GenerateRandomOTP(); // Implement this method

        // Update the user's record in the database with the generated OTP
        ApplicationUser user = _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber).Result;
        if (user != null)
        {
            user.OTP = otp;
            var updateResult = _userManager.UpdateAsync(user).Result;
            if (updateResult.Succeeded)
            {
                phoneNumber = "+91" + phoneNumber;
                // Send the OTP via SMS
                var message = MessageResource.Create(
                    body: $"Your OTP is: {otp}",
                    from: new Twilio.Types.PhoneNumber("+16628835990"),
                    to: new Twilio.Types.PhoneNumber(phoneNumber)
                );

                Console.WriteLine($"OTP sent with Message SID: {message.Sid}");
                return Json(new { success = true, message = "OTP sent" });
            }
            else
            {
                return Json(new { success = false, message = "Failed to update OTP. Please try again." });
            }
        }
        else
        {
            return Json(new { success = false, message = "User not found" });
        }
    }

    private string GenerateRandomOTP()
    {
        const int otpLength = 6; // Set the desired length of the OTP

        // Define the characters allowed in the OTP
        const string characters = "0123456789";

        StringBuilder otp = new StringBuilder(otpLength);
        Random random = new Random();

        for (int i = 0; i < otpLength; i++)
        {
            int index = random.Next(characters.Length);
            otp.Append(characters[index]);
        }

        return otp.ToString();
    }


    [HttpPost]
    public async Task<IActionResult> VerifyOTP(string enteredOTP, string phoneNumber)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);


        if (user != null)
        {
            string storedOTP = user.OTP;// Implement this method

            // Compare the entered OTP with the stored OTP
            if (!string.IsNullOrEmpty(storedOTP) && enteredOTP == storedOTP)
            {
                // Manually sign in the user without redirecting login page
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("User logged in. UserName: {UserName}", user.UserName);
                user.OTP = string.Empty;
                await _userManager.UpdateAsync(user);

                return Json(new { success = true });

            }
        }
        else
        {
            return Json(new { success = false, message = "User not found" });
        }
        return Json(new { success = false, message = "Invalid OTP" });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

