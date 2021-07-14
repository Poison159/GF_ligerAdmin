using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebOneApp.Models
{
    public class Reservation
    {
        public int id { get; set; }

        [Required]
        public int quantity { get; set; }

        public string name { get; set; }

        public int branchId { get; set; }

        public string email { get; set; }

        public string phoneNumber { get; set; }

        [DataType(DataType.DateTime, ErrorMessage = "Time only")]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime timeReservedAt { get; set; }

        [DataType(DataType.DateTime, ErrorMessage = "Date only")]
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime dateReservedAt { get; set; }

        public int spotsLeft { get; set; }

        public string status { get; set; }

        public DateTime processedDateTime { get; set; }

        [DataType(DataType.MultilineText)]
        public string comment { get; set; }

    }

    //public enum ReservationStatus
    //{
    //    AwaitingConfirmation = 1,
    //    Confirmed = 2,
    //    Cancelled = 3,
    //    Rejected = 4
    //}
}