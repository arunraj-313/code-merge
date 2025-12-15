using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace StayEasePG.Models
{
    public class InitialBookingModel
    {
        // PG & Room identifiers
        public int PGID { get; set; }
        public int RoomID { get; set; }
        // PG & Room context (read-only)
        public string PGName { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public string RoomType { get; set; }
        // Pricing details for selected room (read-only)
        public decimal PricePerDay { get; set; }
        public decimal PricePerWeek { get; set; }
        public decimal PricePerMonth { get; set; }
        public decimal Maintenance { get; set; }
        public decimal Advance { get; set; }
        public decimal Deposit { get; set; }
        // User input (Mandatory for POST)
        [Required(ErrorMessage = "Please select a booking duration.")]
        [Display(Name = "Booking Duration")]
        public string SelectedBookingType { get; set; } // Day, Week, or Month
        [DataType(DataType.Date)]
        [Display(Name = "Check-in Date")]
        public DateTime CheckInDate { get; set; } = DateTime.Today;
        // Calculated Final Amounts (set in the POST action)
        public decimal TotalAmount { get; set; }
    }
}