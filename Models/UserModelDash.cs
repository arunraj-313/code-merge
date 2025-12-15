using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StayEasePG.Models
{
    public class UserModelDash
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = "N/A";
        public string Email { get; set; }
        public string Gender { get; set; } = "N/A";
        public string PhoneNo { get; set; } = "N/A";
        public string IDProofType { get; set; }
        public string IDProofNumber { get; set; }
        public string Address { get; set; } = "N/A";
        public string BookingStatus { get; set; }
        public string CheckInStatus { get; set; }

        public List<BookingModel> Bookings { get; set; } = new List<BookingModel>();
        public string Message { get; set; }
    }
}