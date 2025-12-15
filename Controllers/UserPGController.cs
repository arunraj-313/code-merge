//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Data.SqlClient;
//using System.Web.Mvc;
//using StayEasePG.Models;
//namespace StayEasePG.Controllers
//{
//    public class UserPGController : Controller
//    {
//        private string con = ConfigurationManager.ConnectionStrings["StayEasePGConn"].ConnectionString;
//        // ----------------------------------------------------------
//        // 1) PG LIST PAGE (HOME)
//        // ----------------------------------------------------------
//        public ActionResult Index()
//        {
//            List<PGListingViewModel> list = new List<PGListingViewModel>();
//            using (SqlConnection connection = new SqlConnection(con))
//            {
//                string q = @"SELECT PGID, PGName, PGType, PGCategory, City, State, Landmark
//                            FROM PG";
//                SqlCommand cmd = new SqlCommand(q, connection);
//                connection.Open();
//                SqlDataReader dr = cmd.ExecuteReader();
//                while (dr.Read())
//                {
//                    list.Add(new PGListingViewModel
//                    {
//                        PGID = Convert.ToInt32(dr["PGID"]),
//                        PGName = dr["PGName"].ToString(),
//                        PGType = dr["PGType"].ToString(),
//                        Category = dr["PGCategory"].ToString(),
//                        City = dr["City"].ToString(),
//                        State = dr["State"].ToString(),
//                        Landmark = dr["Landmark"].ToString(),
//                    });
//                }
//                dr.Close();
//            }
//            return View(list);
//        }
//        // ----------------------------------------------------------
//        // 2) PG DETAILS PAGE
//        // ----------------------------------------------------------
//        public ActionResult Details(int id)
//        {
//            PGDetailsViewModel model = new PGDetailsViewModel();
//            model.Rooms = new List<RoomDetailsModel>();
//            model.Amenities = new List<string>();
//            using (SqlConnection connection = new SqlConnection(con))
//            {
//                connection.Open();
//                // ---------------- PG Basic Info ----------------
//                SqlCommand pgCmd = new SqlCommand(
//                    "SELECT * FROM PG WHERE PGID = @id", connection);
//                pgCmd.Parameters.AddWithValue("@id", id);
//                SqlDataReader dr = pgCmd.ExecuteReader();
//                if (dr.Read())
//                {
//                    model.PGID = id;
//                    model.PGName = dr["PGName"].ToString();
//                    model.Description = dr["Description"].ToString();
//                    model.Address = dr["Address"].ToString();
//                }
//                dr.Close();
//                // ---------------- Rooms ----------------
//                SqlCommand roomCmd = new SqlCommand(
//                    "SELECT * FROM RoomDetails WHERE PGID = @id", connection);
//                roomCmd.Parameters.AddWithValue("@id", id);
//                SqlDataReader dr2 = roomCmd.ExecuteReader();
//                while (dr2.Read())
//                {
//                    model.Rooms.Add(new RoomDetailsModel
//                    {
//                        RoomID = Convert.ToInt32(dr2["RoomID"]),
//                        RoomType = dr2["RoomType"].ToString(),
//                        TotalRooms = Convert.ToInt32(dr2["TotalRooms"]),
//                        AvailableRooms = Convert.ToInt32(dr2["AvailableRooms"]),
//                        PricePerDay = dr2.GetDecimal(dr2.GetOrdinal("PricePerDay")),
//                        PricePerWeek = dr2.GetDecimal(dr2.GetOrdinal("PricePerWeek")),
//                        PricePerMonth = dr2.GetDecimal(dr2.GetOrdinal("PricePerMonth"))
//                    });
//                }
//                dr2.Close();
//                // ---------------- Amenities ----------------
//                SqlCommand amenityCmd = new SqlCommand(
//                    "SELECT AmenityName FROM Amenities WHERE PGID = @id", connection);
//                amenityCmd.Parameters.AddWithValue("@id", id);
//                SqlDataReader dr3 = amenityCmd.ExecuteReader();
//                while (dr3.Read())
//                {
//                    model.Amenities.Add(dr3["AmenityName"].ToString());
//                }
//                dr3.Close();
//                // ---------------- Rules ----------------
//                SqlCommand ruleCmd = new SqlCommand(
//                    "SELECT * FROM PG_Rules WHERE PGID = @id", connection);
//                ruleCmd.Parameters.AddWithValue("@id", id);
//                SqlDataReader dr4 = ruleCmd.ExecuteReader();
//                if (dr4.Read())
//                {
//                    model.Rules = new PG_Rules
//                    {
//                        RuleID = Convert.ToInt32(dr4["RuleID"]),
//                        PGID = Convert.ToInt32(dr4["PGID"]),
//                        RoomID = Convert.ToInt32(dr4["RoomID"]),
//                        CheckInTime = dr4["CheckInTime"].ToString(),
//                        CheckOutTime = dr4["CheckOutTime"].ToString(),
//                        Restrictions = dr4["Restrictions"].ToString(),
//                        VisitorsAllowed = Convert.ToBoolean(dr4["VisitorsAllowed"]),
//                        GateClosingTime = dr4["GateClosingTime"].ToString(),
//                        NoticePeriod = dr4["NoticePeriod"].ToString()
//                    };
//                }
//                dr4.Close();
//            }
//            return View(model);
//        }
//        // ----------------------------------------------------------
//        // 3) BOOKING PAGE (GET)
//        // ----------------------------------------------------------
//        public ActionResult Book(int pgid, int roomid)
//        {
//            BookingViewModel model = new BookingViewModel();
//            model.PGID = pgid;
//            model.RoomID = roomid;
//            using (SqlConnection conx = new SqlConnection(con))
//            {
//                conx.Open();
//                // Get PG Name
//                SqlCommand cmd = new SqlCommand(
//                    "SELECT PGName FROM PG WHERE PGID = @pgid", conx);
//                cmd.Parameters.AddWithValue("@pgid", pgid);
//                model.PGName = cmd.ExecuteScalar()?.ToString();
//                // Get Room Type
//                SqlCommand cmd2 = new SqlCommand(
//                    "SELECT RoomType FROM RoomDetails WHERE RoomID = @roomid", conx);
//                cmd2.Parameters.AddWithValue("@roomid", roomid);
//                model.RoomType = cmd2.ExecuteScalar()?.ToString();
//            }
//            return View(model);
//        }
//        // ----------------------------------------------------------
//        // 4) BOOKING PAGE (POST)
//        // ----------------------------------------------------------
//        [HttpPost]
//        public ActionResult Book(BookingViewModel model)
//        {
//            using (SqlConnection conx = new SqlConnection(con))
//            {
//                conx.Open();
//                SqlCommand cmd = new SqlCommand(
//                    @"INSERT INTO Booking
//                     (PGID, RoomID, CheckInDate, BookingType, Duration, TotalAmount,
//                      PaymentStatus, BookingStatus)
//                     VALUES
//                     (@PGID, @RoomID, @CheckInDate, @BookingType, @Duration, @TotalAmount,
//                      @PaymentStatus, @BookingStatus)", conx);
//                cmd.Parameters.AddWithValue("@PGID", model.PGID);
//                cmd.Parameters.AddWithValue("@RoomID", model.RoomID);
//                cmd.Parameters.AddWithValue("@CheckInDate", model.CheckInDate);
//                cmd.Parameters.AddWithValue("@BookingType", model.BookingType);
//                cmd.Parameters.AddWithValue("@Duration", model.Duration);
//                cmd.Parameters.AddWithValue("@TotalAmount", model.TotalAmount);
//                cmd.Parameters.AddWithValue("@PaymentStatus", model.PaymentStatus);
//                cmd.Parameters.AddWithValue("@BookingStatus", model.BookingStatus);
//                cmd.ExecuteNonQuery();
//            }
//            return RedirectToAction("Index");
//        }
//    }
//}