using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebOneApp.Models
{
    public class BranchMeal
    {
        public BranchMeal()
        {
            imgPath = "~/Content/user.png";
        }
        public int id { get; set; }
        [Required]
        [Display(Name = "Restaurant")]
        public int branchId { get; set; }
        [Required]
        [Display(Name = "Category")]
        public string category { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string name { get; set; }

        [Required]
        [Display(Name = "Price")]
        public decimal price { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start Date")]
        public DateTime startDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:yyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime endDate { get; set; }

        [Required]
        [Display(Name = "Image")]
        public string imgPath { get; set; }

        [NotMapped]
        public HttpPostedFileBase imageUpload { get; set; }
    }
}