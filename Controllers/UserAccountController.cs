using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using StayEasePG.Models;
namespace StayEasePG.Controllers
{
    public class UserAccountController : Controller
    {
        string cs = ConfigurationManager.ConnectionStrings["StayEasePGConn"].ConnectionString;
        public ActionResult AdminUserAccounts()
        {
            if (Session["AdminID"] == null)
                return RedirectToAction("AdminLogin", "Admin");
            int adminId = Convert.ToInt32(Session["AdminID"]);
            List<UserAccountModel> list = new List<UserAccountModel>();
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                string query = @"
                   SELECT
                       b.BookingID,
                       u.FullName AS UserName,
                       u.Email,
                       u.PhoneNo AS Phone,
                       u.IDProofType + ' - ' + u.IDProofNumber AS IDProof,
                       u.Address,
                       r.RoomType,
                       b.BookingType,
                       b.TotalAmount AS Amount,
                       b.BookingStatus
                   FROM Booking b
                   JOIN Users u ON b.UserID = u.UserID
                   JOIN RoomDetails r ON b.RoomID = r.RoomID
                   JOIN PG p ON b.PGID = p.PGID
                   WHERE p.AdminID = @adminId
                   AND b.BookingStatus = 'CheckedIn'   -- ONLY CHECKED-IN USERS
                   ORDER BY b.BookingID DESC";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@adminId", adminId);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new UserAccountModel
                    {
                        BookingID = Convert.ToInt32(dr["BookingID"]),
                        UserName = dr["UserName"].ToString(),
                        Email = dr["Email"].ToString(),
                        Phone = dr["Phone"].ToString(),
                        IDProof = dr["IDProof"].ToString(),
                        Address = dr["Address"].ToString(),
                        RoomType = dr["RoomType"].ToString(),
                        BookingType = dr["BookingType"].ToString(),
                        Amount = Convert.ToDecimal(dr["Amount"]),
                        Status = dr["BookingStatus"].ToString()
                    });
                }
            }
            return View(list);
        }
    }
}