using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StayEasePG.Models
{
    public class PG_Rules
    {
        public int RuleID { get; set; }
        public int PGID { get; set; }
        public int RoomID { get; set; }

        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public string Restrictions { get; set; }
        public bool VisitorsAllowed { get; set; }
        public string GateClosingTime { get; set; }
        public string NoticePeriod { get; set; }

        // Navigation Properties
        public virtual PG PG { get; set; }
        public virtual RoomDetails RoomDetails { get; set; }
    }
}