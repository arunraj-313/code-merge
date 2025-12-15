using System;

using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;

using System.Linq;

using System.Web;

using System.Web.Mvc;

namespace StayEasePG.Models

{

    public class RoomDetails

    {

        [Key]

        public int RoomID { get; set; }

        [Required(ErrorMessage = "Please select a PG.")]

        [Display(Name = "PG Name & Location")]

        public int PGID { get; set; } // Foreign key to PG table

        [Required(ErrorMessage = "Please select a Room Type.")]

        [Display(Name = "Room Type (Sharing)")]

        public string RoomType { get; set; } // Single, Double Sharing, etc.

        [Required]

        [Display(Name = "Total Rooms")]

        public int TotalRooms { get; set; }

        [Required]

        [Display(Name = "Available Rooms")]

        public int AvailableRooms { get; set; }

        [Required]

        [Display(Name = "Price per Day")]

        public decimal PricePerDay { get; set; }

        [Required]

        [Display(Name = "Price per Week")]

        public decimal PricePerWeek { get; set; }

        [Required]

        [Display(Name = "Price per Month")]

        public decimal PricePerMonth { get; set; }

        [Display(Name = "Maintenance Charges")]

        public decimal? MaintenanceCharges { get; set; }

        [Display(Name = "Advance Amount")]

        public decimal? AdvanceAmount { get; set; }

        [Display(Name = "Deposit Amount")]

        public decimal? DepositAmount { get; set; }

        // --- NEW PROPERTIES FOR DROPDOWNS ---

        // This list will hold the PG Name/Location options retrieved by the Controller.

        public List<SelectListItem> PGOptions { get; set; }

        // This list holds the fixed sharing options (Single, Double, etc.).

        public List<SelectListItem> RoomTypeOptions { get; set; }

        // For selected amenities in Add/Edit views

        [Display(Name = "Amenities")]

        public List<int> SelectedAmenities { get; set; } = new List<int>();

    }

    public class Amenity

    {

        public int AmenityID { get; set; }

        public string AmenityName { get; set; }

    }

}
