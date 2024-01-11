using System;
using System.ComponentModel.DataAnnotations;

namespace HobbyHarbour.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Category Name is required.")]
        [MaxLength(30)]
        public string CategoryName { get; set; }

        public string Description { get; set; }

        public bool IsDeleted { get; set; }

        public Category()
        {
            IsDeleted = false; 
        }
    }

}



