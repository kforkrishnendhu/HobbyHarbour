using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace HobbyHarbour.Models
{
   
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(1, 10000)]
        public decimal Price { get; set; }

        [Required]
        [Range(1, 1000)]
        public int StockQuantity { get; set; }

        public int? CategoryID { get; set; }

        public Category? Category { get; set; }

        [Display(Name = "Images to Delete")]
        [NotMapped]
        public List<int> ImagesToDelete { get; set; }

        public List<ProductImage> Images { get; set; }

        public bool IsDeleted { get; set; }

        public Product()
        {
            IsDeleted = false;
            Images = new List<ProductImage>();
            ImagesToDelete = new List<int>();
        }
    }


}

