using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StayEasePG.Models
{
    public class PGListingViewModel
    {
        public int UserID { get; set; }
        public int PGID { get; set; }
        public string PGName { get; set; }
        public string PGType { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Landmark { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal PricePerWeek { get; set; }
        public decimal PricePerMonth { get; set; }
    }
}