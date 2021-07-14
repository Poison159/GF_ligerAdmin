using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebOneApp.Models
{
    public class OperatingHours
    {
        public int id { get; set; }
        [Required]
        public int branchId { get; set; }
        [Required]
        public string day { get; set; }
        [Required]
        [Display(Name = "opening hour")]
        [DataType(DataType.Time)]
        public DateTime openingHour { get; set; }
        [Required]
        [Display(Name = "closing hour")]
        [DataType(DataType.Time)]
        public DateTime closingHour { get; set; }
    }
}