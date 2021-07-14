using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebOneApp.Models
{
    public class Manager
    {
        public string email { get; set; }
        public string password { get; set; }
        public string confirmPassword { get; set; }
        public string role { get; set; }
    }
}