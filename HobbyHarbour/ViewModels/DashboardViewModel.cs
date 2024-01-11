using System;

namespace HobbyHarbour.ViewModels
{
    public class DashboardViewModel
    {
        public string SalesLineChartData { get; set; }
        public string PaymentMethodPieChartData { get; set; }
        public string ProductSalesBarChartData { get; set; }
        public int? SelectedYear { get; set; }
        public int? SelectedMonth { get; set; }
    }
}

