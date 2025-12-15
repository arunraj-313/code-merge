using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StayEasePG.Models
{
    public class PGDetailsViewModel
    {
        public int UserID { get; set; }
        public int PGID { get; set; }
        public string PGName { get; set; }
        public string PGType { get; set; }
        public string PGCategory { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Landmark { get; set; }
        public string Description { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal PricePerWeek { get; set; }
        public decimal PricePerMonth { get; set; }
        public List<RoomDetailsModel> Rooms { get; set; }


    }
}