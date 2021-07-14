using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebOneApp.Models
{
    public class Resturant
    {
        public Resturant()
        {
            imgPath = "~/Content/user.png";
            meals = new Dictionary<string,List<Meal>>();
            categories = new List<string>();
            guid = Guid.NewGuid().ToString().Split('-').First();
        }

        [Required]
        public int id { get; set; }
        [Required]
        public string guid { get; set; }
        [Required]
        [Display(Name = "Name")]
        public string name { get; set; }
        [Required]
        [Display(Name = "Image")]
        public string imgPath { get; set; }
        [NotMapped]
        public HttpPostedFileBase imageUpload { get; set; }
        public Dictionary<string,List<Meal>> meals { get; set; }
        public List<string> categories { get; set; }
    }
}