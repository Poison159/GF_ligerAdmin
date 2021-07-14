using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebOneApp.Models
{
    public class Rating
    {
        public int id { get; set; }
        public int userId { get; set; }
        public int branchId { get; set; }
        public int rating { get; set; }
        public string comment { get; set; }
        [NotMapped]
        public string username { get; set; }
    }
}