using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebOneApp.Models
{
    public class Token
    {
        public Token() { }
        public Token(string userId, string access_token, DateTime grantDate, DateTime expiryDate)
        {
            _access_token = access_token;
            _userId = userId;
            _grantDate = grantDate;
            _expiryDate = expiryDate;
        }
        public int id { get; set; }
        public string _userId { get; set; }
        public string _access_token { get; set; }
        public DateTime _grantDate { get; set; }
        public DateTime _expiryDate { get; set; }
    }
}