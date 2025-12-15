using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace StayEasePG.Models
{
    public class FoodMenu
    {
        [Key]
        public int MenuID { get; set; }
        [Required]
        [Display(Name = "PG")]
        public int PGID { get; set; }
        [Required]
        [Display(Name = "Room")]
        public int RoomID { get; set; }
        [Required]
        [Display(Name = "Day of Week")]
        public string DayOfWeek { get; set; }  // Monday - Sunday
        [Required]
        [Display(Name = "Meal Type")]
        public string MealType { get; set; }   // Breakfast, Lunch, Dinner
        [Required]
        [Display(Name = "Menu Items")]
        public string MenuItems { get; set; }  // e.g., "Idli, Dosa, Sambhar"
    }
}