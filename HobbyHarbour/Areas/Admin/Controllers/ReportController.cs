using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using HobbyHarbour.Data;
using HobbyHarbour.ViewModels;
using LiveCharts.Wpf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Helpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HobbyHarbour.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly PdfGenerator _pdfGenerator;

        public ReportController(PdfGenerator pdfGenerator, ApplicationDbContext db)
        {
            _pdfGenerator = pdfGenerator;
            _db = db;
        }

        public IActionResult SalesReport(int? selectedYear, int? selectedMonth, int page=1, int pageSize = 12)
        {
            selectedYear ??= DateTime.Now.Year;
            selectedMonth ??= DateTime.Now.Month;

            var startDate = new DateTime(selectedYear.Value, selectedMonth.Value, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var totalProducts = _db.Orders.Count();
            var ordersData = _db.Orders
                         .Include(o => o.LineItems)
                         .ThenInclude(o => o.Product)
                         .Where(s => s.OrderDate >= startDate && s.OrderDate <= endDate)
                         .OrderBy(s => s.OrderDate)
                         .Skip((page - 1) * pageSize)
                         .Take(pageSize)
                         .ToList();

            // Pass the paginated products and pagination information to the view
            var viewModel = new SalesIndexViewModel
            {
                Orders = ordersData,
                PageInfo = new PageInfo { CurrentPage = page, ItemsPerPage = pageSize, TotalItems = totalProducts },
                SelectedYear = selectedYear,
                SelectedMonth = selectedMonth
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_SalesTablePartial", viewModel);
            }

            return View(viewModel);
        }

        public IActionResult DownloadPdf()
        {
            var ordersData = _db.Orders
                                .Include(o => o.LineItems)
                                .ThenInclude(o => o.Product)
                                .ToList();
            var pdfBytes = _pdfGenerator.GenerateSalesReport(ordersData);
            return File(pdfBytes, "application/pdf", "SalesReport.pdf");
        }

        public IActionResult DownloadExcel()
        {
            var ordersData = _db.Orders
                                .Include(o => o.LineItems)
                                .ThenInclude(o => o.Product)
                                .ToList();

            // Create a new Excel package
            using (var package = new ExcelPackage())
            {
                // Add a new worksheet
                var worksheet = package.Workbook.Worksheets.Add("SalesReport");

                // Add headers
                worksheet.Cells["A1"].Value = "Order ID";
                worksheet.Cells["B1"].Value = "Order Date";
                worksheet.Cells["C1"].Value = "Total Amount";

                // Set header style
                using (var range = worksheet.Cells["A1:C1"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Populate data
                var row = 2;
                foreach (var order in ordersData)
                {
                    worksheet.Cells[row, 1].Value = order.OrderID;
                    worksheet.Cells[row, 2].Value = order.OrderDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 3].Value = order.TotalAmount;

                    row++;
                }

                // Auto-fit columns for better visibility
                worksheet.Cells.AutoFitColumns();

                // Convert the Excel package to a byte array
                var excelBytes = package.GetAsByteArray();

                // Return the Excel file
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SalesReport.xlsx");
            }
        }
    }
}

