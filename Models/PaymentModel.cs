using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StayEasePG.Models
{
    public class PaymentModel
    {
        public int BookingID { get; set; }
        public string UserName { get; set; }
        public string IDProof { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string RoomType { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public string BookingType { get; set; }
        public string Payment { get; set; }
        public decimal Amount { get; set; }
        public string BookingStatus { get; set; }
    }
}