using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace StayEasePG.Models
{
    public class BookingModel
    {
        // Fields for Database insertion
        public int BookingID { get; set; }
        public int UserID { get; set; } // Assumed to be populated from session/login
        public int PGID { get; set; }
        public int RoomID { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CheckInDate { get; set; }
        public string BookingType { get; set; } // Day, Week, Month

        public decimal TotalAmount { get; set; }
        // Renamed from PaymentStatus to PaymentMethod based on user request
        public string PaymentMethod { get; set; }
        public string BookingStatus { get; set; }
        // --- Fields for View Context (READ-ONLY) ---
        // These are populated in the GET controller action
        public string PGName { get; set; }
        public string City { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public string RoomType { get; set; }
        public decimal Maintenance { get; set; }
        public decimal Advance { get; set; }
        public decimal Deposit { get; set; }
        // Helper properties for calculation (to be populated in GET action)
        public decimal PricePerDay { get; set; }
        public decimal PricePerWeek { get; set; }
        public decimal PricePerMonth { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public int Duration { get; set; }
    }
}