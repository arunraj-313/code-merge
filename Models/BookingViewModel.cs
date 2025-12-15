using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StayEasePG.Models
{
    public class BookingViewModel
    {
        public int UserID { get; set; }
        public int BookingID { get; set; }
        public int PGID { get; set; }
        public int RoomID { get; set; }
        public string PGName { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string RoomType { get; set; }
        public DateTime CheckInDate { get; set; }
        public string BookingType { get; set; }  // Day, Week, Month
        public int Duration { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string BookingStatus { get; set; }
        public string CheckInStatus { get; set; }
    }
}