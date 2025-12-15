using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using StayEasePG.Models;
namespace StayEasePG.Controllers
{
    public class PaymentController : Controller
    {
        string cs = ConfigurationManager.ConnectionStrings["StayEasePGConn"].ConnectionString;
        // ================================
        //  ADMIN PAYMENT LIST VIEW
        // ================================
        public ActionResult AdminPayments()
        {
            if (Session["AdminID"] == null)
                return RedirectToAction("AdminLogin", "Admin");
            List<PaymentModel> list = new List<PaymentModel>();
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                string query = @"
                   SELECT
                       b.BookingID,
                       u.FullName AS UserName,
                       u.IDProofType + ' - ' + u.IDProofNumber AS IDProof,
                       u.PhoneNo AS Phone,
                       u.Address,
                       r.RoomType,
                       b.CheckInDate,
                       b.CheckOutDate,
                       b.BookingType,
                       b.PaymentMethod AS Payment,
                       b.TotalAmount AS Amount,
                       b.BookingStatus
                   FROM Booking b
                   JOIN Users u ON b.UserID = u.UserID
                   JOIN RoomDetails r ON b.RoomID = r.RoomID
                   ORDER BY b.BookingID DESC";
                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new PaymentModel
                    {
                        BookingID = Convert.ToInt32(dr["BookingID"]),
                        UserName = dr["UserName"].ToString(),
                        IDProof = dr["IDProof"].ToString(),  // ✔ FIXED
                        Phone = dr["Phone"].ToString(),
                        Address = dr["Address"].ToString(),
                        RoomType = dr["RoomType"].ToString(),
                        CheckInDate = Convert.ToDateTime(dr["CheckInDate"]),
                        CheckOutDate = dr["CheckOutDate"] == DBNull.Value ? null : (DateTime?)dr["CheckOutDate"],
                        BookingType = dr["BookingType"].ToString(),
                        Payment = dr["Payment"].ToString(),
                        Amount = Convert.ToDecimal(dr["Amount"]),
                        BookingStatus = dr["BookingStatus"].ToString()
                    });
                }
            }
            return View(list);
        }

        // ================================
        //  CHECK-IN ACTION
        // ================================
        public ActionResult CheckIn(int id)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                string query = @"
           UPDATE Booking
           SET BookingStatus = 'CheckedIn',
               CheckInStatus = 'CheckedIn'
           WHERE BookingID = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            TempData["Success"] = "User Checked-In Successfully.";
            return RedirectToAction("AdminPayments");
        }
        // ================================
        //  CHECK-OUT ACTION
        // ================================
        public ActionResult CheckOut(int id)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                SqlTransaction tx = con.BeginTransaction();
                try
                {
                    // 1. Update booking status, check-in status and checkout date
                    string query = @"
               UPDATE Booking
               SET BookingStatus = 'CheckedOut',
                   CheckInStatus = 'CheckedOut',
                   CheckOutDate = GETDATE()
               WHERE BookingID = @id";
                    SqlCommand cmd = new SqlCommand(query, con, tx);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    // 2. Increment room availability
                    string updateRoom = @"
               UPDATE RoomDetails
               SET AvailableRooms = AvailableRooms + 1
               WHERE RoomID = (SELECT RoomID FROM Booking WHERE BookingID = @id)";
                    SqlCommand cmdRoom = new SqlCommand(updateRoom, con, tx);
                    cmdRoom.Parameters.AddWithValue("@id", id);
                    cmdRoom.ExecuteNonQuery();
                    tx.Commit();
                    TempData["Success"] = "User Checked-Out Successfully and room is now available.";
                }
                catch
                {
                    tx.Rollback();
                    TempData["Error"] = "Check-Out failed. Try again.";
                }
            }
            return RedirectToAction("AdminPayments");
        }
    }
}