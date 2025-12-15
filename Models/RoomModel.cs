using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StayEasePG.Models
{
    public class RoomModel
    {
        public int RoomID { get; set; }
        public int PGID { get; set; }
        public string RoomType { get; set; }
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal PricePerWeek { get; set; }
        public decimal PricePerMonth { get; set; }
    }
}