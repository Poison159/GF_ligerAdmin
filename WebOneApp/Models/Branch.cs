using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebOneApp.Models
{
    public class Branch
    {
        public Branch() {
            guid = Guid.NewGuid().ToString().Split('-').First();
            operatingHoursStr = new List<string>();
            branchMeals = new List<BranchMeal>();
        }
        public int id { get; set; }
        
        public string name { get; set; }
        [Required]
        [Display(Name="resturant")]
        public int restId { get; set; }
        [Required]
        [Display(Name = "manager")]
        public string userId { get; set; }
        [Required]
        public string guid { get; set; }
        [Required]
        public string address { get; set; }
        public double rating { get; set; }
        [Required]
        public string lat { get; set; }
        [Required]
        public string lon { get; set; }
        [Display(Name = "phone number")]
        public string contactNumber { get; set; }
        [NotMapped]
        public double distance { get; set; }
        [NotMapped]
        public string info { get; set; }
        [NotMapped]
        public string openOrClosedInfo { get; set; }
        public List<string> operatingHoursStr { get; set; }
        [NotMapped]
        public bool open { get; set; }
        [NotMapped]
        public bool closingSoon { get; set; }
        [NotMapped]
        public bool openingSoon { get; set; }
        public List<BranchMeal> branchMeals { get; set; }
        [NotMapped]
        public Resturant resturant { get; set; }
        public List<Rating> reviews { get; set; }

        public int reservationSpot { get; set; }
    }
}