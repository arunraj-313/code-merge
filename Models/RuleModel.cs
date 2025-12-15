using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StayEasePG.Models
{
    public class RuleModel
    {
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public string Restrictions { get; set; }
        public bool VisitorsAllowed { get; set; }
        public string GateClosingTime { get; set; }
        public string NoticePeriod { get; set; }
    }
}
