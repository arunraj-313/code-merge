using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using StayEasePG.Models;
namespace StayEasePG.Controllers
{
    public class UserDashboardController : Controller
    {
        string cs = ConfigurationManager.ConnectionStrings["StayEasePGConn"].ConnectionString;
        private bool IsUserLoggedIn()
        {
            return Session["UserID"] != null;
        }
        // -------------------- USER DASHBOARD --------------------
        public ActionResult Index()
        {
            if (!IsUserLoggedIn())
            {
                TempData["Error"] = "Please login to access your dashboard.";
                return RedirectToAction("Login", "Account");
            }
            int userId = Convert.ToInt32(Session["UserID"]);
            List<BookingViewModel> list = new List<BookingViewModel>();
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                string q = @"
                   SELECT b.BookingID, b.PGID, b.RoomID, b.PaymentStatus, b.PaymentMethod,
                          b.CheckInStatus,
                          pg.PGName, pg.City, pg.Address,
                          r.RoomType
                   FROM Booking b
                   INNER JOIN PG pg ON b.PGID = pg.PGID
                   INNER JOIN RoomDetails r ON b.RoomID = r.RoomID
                   WHERE b.UserID = @userId
                   ORDER BY b.BookingID DESC";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@userId", userId);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new BookingViewModel
                    {
                        BookingID = Convert.ToInt32(dr["BookingID"]),
                        PGID = Convert.ToInt32(dr["PGID"]),
                        PGName = dr["PGName"].ToString(),
                        City = dr["City"].ToString(),
                        Address = dr["Address"].ToString(),
                        RoomID = Convert.ToInt32(dr["RoomID"]),
                        RoomType = dr["RoomType"].ToString(),
                        PaymentStatus = dr["PaymentStatus"].ToString(),
                        PaymentMethod = dr["PaymentMethod"].ToString(),
                        CheckInStatus = dr["CheckInStatus"].ToString()
                    });
                }
            }
            return View(list);
        }
        // -------------------- CANCEL BOOKING --------------------
        public ActionResult CancelBooking(int bookingId, int roomId)
        {
            if (!IsUserLoggedIn())
            {
                TempData["Error"] = "Please login to cancel a booking.";
                return RedirectToAction("Login", "Account");
            }
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                // Check if already CheckedIn
                SqlCommand checkCmd = new SqlCommand(
                    "SELECT CheckInStatus FROM Booking WHERE BookingID=@id", con);
                checkCmd.Parameters.AddWithValue("@id", bookingId);
                string status = checkCmd.ExecuteScalar()?.ToString();
                if (status == "CheckedIn")
                {
                    TempData["Error"] = "You cannot cancel after Check-In.";
                    return RedirectToAction("Index");
                }
                SqlTransaction tx = con.BeginTransaction();
                try
                {
                    // 1. Cancel booking
                    string q1 = @"UPDATE Booking
                                 SET BookingStatus='Cancelled', PaymentStatus='Refunded'
                                 WHERE BookingID=@bookingId";
                    SqlCommand cmd1 = new SqlCommand(q1, con, tx);
                    cmd1.Parameters.AddWithValue("@bookingId", bookingId);
                    cmd1.ExecuteNonQuery();
                    // 2. Increase available rooms
                    string q2 = @"UPDATE RoomDetails
                                 SET AvailableRooms = AvailableRooms + 1
                                 WHERE RoomID=@roomId";
                    SqlCommand cmd2 = new SqlCommand(q2, con, tx);
                    cmd2.Parameters.AddWithValue("@roomId", roomId);
                    cmd2.ExecuteNonQuery();
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    TempData["Error"] = "Cancellation failed. Try again.";
                }
            }
            return RedirectToAction("Index");
        }
    }
}