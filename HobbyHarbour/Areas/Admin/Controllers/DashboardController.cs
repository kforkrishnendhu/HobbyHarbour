using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using HobbyHarbour.Data;
using HobbyHarbour.ViewModels;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HobbyHarbour.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        public DashboardController(ApplicationDbContext db)
        {
            _db = db;
        }

        public ActionResult Dashboard(int? selectedYear, int? selectedMonth)
        {
            selectedYear ??= DateTime.Now.Year;
            selectedMonth ??= DateTime.Now.Month;

            var startDate = new DateTime(selectedYear.Value, selectedMonth.Value, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var salesData = _db.Orders
                .Where(s => s.OrderDate >= startDate && s.OrderDate <= endDate)
                .OrderBy(s => s.OrderDate)
                .ToList();

            // Convert the salesData to a format suitable for the line chart
            var salesLineChartData = new
            {
                labels = salesData.Select(s => s.OrderDate.ToString("yyyy-MM-dd")),
                datasets = new[]
                {
                    new
                    {
                        label = "Total Sales",
                        data = salesData.Select(s => s.TotalAmount),
                        borderColor = "rgba(75, 192, 192, 1)",
                        borderWidth = 2,
                        fill = false
                    }
                }
            };

            var paymentMethodData = _db.Payments
                                    .Where(p => p.Date >= startDate && p.Date <= endDate)
                                    .GroupBy(s => s.PaymentMethod)
                                    .Select(group => new
                                    {
                                        PaymentMethod = group.Key,
                                        Count = group.Count()
                                    })
                                    .ToList();

            // Convert the paymentMethodData to a format suitable for the pie chart
            var paymentMethodPieChartData = new
            {
                labels = paymentMethodData.Select(p => p.PaymentMethod),
                datasets = new[]
                {
            new
            {
                data = paymentMethodData.Select(p => p.Count),
                backgroundColor = new[] { "#FF6384", "#36A2EB", "#FFCE56" },
                hoverBackgroundColor = new[] { "#FF6384", "#36A2EB", "#FFCE56" }
            }
        }
            };

            var productSalesData = _db.OrderItems
                                .Where(p => p.Order.OrderDate >= startDate && p.Order.OrderDate <= endDate)
                                .GroupBy(s => s.Product.ProductID)
                                .Select(group => new
                                {
                                    ProductName = group.Key,
                                    TotalQuantitySold = group.Sum(s => s.Quantity)
                                })
                                .ToList();

            // Convert the productSalesData to a format suitable for the bar chart
            var productSalesBarChartData = new
            {
                labels = productSalesData.Select(p => p.ProductName),
                datasets = new[]
                {
            new
            {
                label = "Total Quantity Sold",
                data = productSalesData.Select(p => p.TotalQuantitySold),
                backgroundColor = "#36A2EB"
            }
        }
            };


            var viewModel = new DashboardViewModel
            {
                SalesLineChartData = JsonConvert.SerializeObject(salesLineChartData),
                PaymentMethodPieChartData = JsonConvert.SerializeObject(paymentMethodPieChartData),
                ProductSalesBarChartData = JsonConvert.SerializeObject(productSalesBarChartData),
                SelectedYear = selectedYear,
                SelectedMonth = selectedMonth
            };

            return View(viewModel);
        }
    }
}

