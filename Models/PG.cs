using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StayEasePG.Models
{
    public class PG
    {
        public int AdminID { get; set; }
        public int PGID { get; set; }
        public string PGName { get; set; }
        public string Email { get; set; }
        public string PGType { get; set; }
        public string PGCategory { get; set; }
        public string Description { get; set; }
        public int LocationID { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PinCode { get; set; }
        public string Landmark { get; set; }

        public List<HttpPostedFileBase> Photos { get; set; }
    }
}