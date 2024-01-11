using System;
using HobbyHarbour.Models;

namespace HobbyHarbour.ViewModels
{
    public class PageInfo
    {
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / ItemsPerPage);
    }

    public class ProductIndexViewModel
    {
        public List<Product> Products { get; set; }
        public PageInfo PageInfo { get; set; }
    }

    public class SalesIndexViewModel
    {
        public List<Order> Orders { get; set; }
        public PageInfo PageInfo { get; set; }
        public int? SelectedYear { get; set; }
        public int? SelectedMonth { get; set; }
    }

    public class OrderIndexViewModel
    {
        public List<Order> Orders { get; set; }
        public PageInfo PageInfo { get; set; }
    }

}

