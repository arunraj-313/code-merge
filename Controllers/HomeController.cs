//using StayEasePG.Models;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

//namespace StayEasePG.Controllers
//{
//    public class HomeController : Controller
//    {
//        public ActionResult Index()
//        {
//            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["StayEasePGConn"].ConnectionString))
//            {
//                con.Open();
//                SqlCommand cmd = new SqlCommand("SELECT PGID, PGName, Address, City FROM PG", con);
//                SqlDataReader dr = cmd.ExecuteReader();
//                List<PG> pgList = new List<PG>();
//                while (dr.Read())
//                {
//                    pgList.Add(new PG
//                    {
//                        PGID = Convert.ToInt32(dr["PGID"]),
//                        PGName = dr["PGName"].ToString(),
//                        Address = dr["Address"].ToString(),
//                        City = dr["City"].ToString()
//                    });
//                }
//                return View(pgList);   // ✔ SEND MODEL
//            }
//        }

//        public ActionResult About()
//        {
//            ViewBag.Message = "Your application description page.";

//            return View();
//        }

//        public ActionResult Contact()
//        {
//            ViewBag.Message = "Your contact page.";

//            return View();
//        }
//    }
//}




using StayEasePG.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace StayEasePG.Controllers
{
    public class HomeController : Controller
    {
        private readonly string connString = ConfigurationManager.ConnectionStrings["StayEasePGConn"].ConnectionString;

        public ActionResult Index(string openLogin = null)
        {
            List<PG> pgList = new List<PG>();

            try
            {
                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT PGID, PGName, Address, City FROM PG", con);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            pgList.Add(new PG
                            {
                                PGID = Convert.ToInt32(dr["PGID"]),
                                PGName = dr["PGName"].ToString(),
                                Address = dr["Address"].ToString(),
                                City = dr["City"].ToString()
                            });
                        }
                    }
                }
            }
            catch
            {
                // ignore errors here; show empty list
            }

            // If Index is called with openLogin query string, pass flag to view via ViewBag
            ViewBag.OpenLogin = (openLogin == "true" || Request.QueryString["openLogin"] == "true");
            // Pass any server-side login error (TempData)
            if (TempData["loginError"] != null)
            {
                ViewBag.ServerLoginError = TempData["loginError"].ToString();
            }
            if (TempData["success"] != null)
            {
                ViewBag.SuccessMessage = TempData["success"].ToString();
            }
            if (TempData["error"] != null)
            {
                ViewBag.ErrorMessage = TempData["error"].ToString();
            }

            return View(pgList);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}
