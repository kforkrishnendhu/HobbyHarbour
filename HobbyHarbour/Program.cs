using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HobbyHarbour.Data;
using HobbyHarbour.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using HobbyHarbour.Service;
using QuestPDF.Infrastructure;
using OfficeOpenXml;

QuestPDF.Settings.License = LicenseType.Community;
ExcelPackage.LicenseContext = LicenseContext.Commercial;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IPaymentService, PaymentService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<PdfGenerator>();


builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddRazorPages();

// Create the admin role if it doesn't exist
var roleManager = builder.Services.BuildServiceProvider().GetRequiredService<RoleManager<IdentityRole>>();
if (!await roleManager.RoleExistsAsync("Admin"))
{
    await roleManager.CreateAsync(new IdentityRole("Admin"));
}
if (!await roleManager.RoleExistsAsync("Customer"))
{
    await roleManager.CreateAsync(new IdentityRole("Customer"));
}
if (!await roleManager.RoleExistsAsync("Home"))
{
    await roleManager.CreateAsync(new IdentityRole("Home"));
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));

    options.AddPolicy("Customer", policy => policy.RequireRole("Customer"));

    options.AddPolicy("Home", policy => policy.RequireRole("Home"));
});

//builder.Services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
builder.Services.AddMvc()
    .AddViewOptions(options =>
    {
        options.HtmlHelperOptions.ClientValidationEnabled = true;
    });



var app = builder.Build();

// Run the initialization logic in a separate scope
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Find the user you want to make an admin by their ID
    var userId = "9cfe2917-fa24-4317-88f0-990b07be70e2"; 
    var user = userManager.FindByIdAsync(userId).Result;

    if (user != null)
    {
        // Assign the "Admin" role to the user
        var result = userManager.AddToRoleAsync(user, "Admin").Result;

        if (result.Succeeded)
        {
            // Role assignment succeeded
        }
        else
        {
            // Handle errors if role assignment fails
        }
    }
    else
    {
        // Handle the case where the user with the specified ID was not found
    }
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// Configure custom error handling for 404 errors
app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapAreaControllerRoute(
    name: "admin",
    areaName: "Admin", // Your Admin area name
    pattern: "Admin/{controller=Dashboard}/{action=Dashboard}/{id?}"
);

app.MapAreaControllerRoute(
    name: "customer",
    areaName: "Customer", // Your Customer area name
    pattern: "Customer/{controller=CustomerHome}/{action=Index}/{id?}"
);

//app.MapAreaControllerRoute(
//    name: "home",
//    areaName: "Home",
//    pattern: "Areas/Home/Controller/{controller=Home}/{action=Index}/{id?}"
//);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
        name: "error",
        pattern: "/Home/Error/{statusCode}",
        defaults: new { controller = "Home", action = "Error" });




app.MapRazorPages();

app.Run();

