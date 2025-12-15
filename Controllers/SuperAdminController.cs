using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using System.Web.Mvc;
using StayEasePG.Controllers;


namespace StayEasePG.Controllers
{
    public class SuperAdminController : Controller
    {
        string connString = ConfigurationManager.ConnectionStrings["StayEasePGConn"].ConnectionString;

        // ===================== DASHBOARD ======================
        public ActionResult Dashboard()
        {
            // Check if logged in
            if (Session["SuperAdmin"] == null)
                return RedirectToAction("Index", "Home", new { openLogin = "true" });

            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connString))
            {
                string q = "SELECT * FROM PendingAdminRequests WHERE Status='Pending'";
                using (SqlDataAdapter da = new SqlDataAdapter(q, con))
                {
                    da.Fill(dt);
                }
            }

            return View(dt);
        }

        // ===================== APPROVE ======================
        public ActionResult Approve(int id)
        {
            using (SqlConnection con = new SqlConnection(connString))
            {
                con.Open();

                // Load pending admin
                string selectQ = "SELECT * FROM PendingAdminRequests WHERE RequestID=@ID";
                SqlCommand selectCmd = new SqlCommand(selectQ, con);
                selectCmd.Parameters.AddWithValue("@ID", id);
                SqlDataReader dr = selectCmd.ExecuteReader();

                if (!dr.Read())
                {
                    TempData["error"] = "Request not found!";
                    return RedirectToAction("PendingAdmins");
                }

                string fullName = dr["FullName"].ToString();
                string email = dr["Email"].ToString();
                string phone = dr["PhoneNo"].ToString();
                string password = dr["PasswordHash"].ToString();
                dr.Close();

                // Insert into Admins
                string insertQ = @"INSERT INTO Admins
                          (FullName, Email, PhoneNo, PasswordHash, Role)
                           VALUES (@F,@E,@P,@PW,'Admin')";
                SqlCommand insertCmd = new SqlCommand(insertQ, con);
                insertCmd.Parameters.AddWithValue("@F", fullName);
                insertCmd.Parameters.AddWithValue("@E", email);
                insertCmd.Parameters.AddWithValue("@P", phone);
                insertCmd.Parameters.AddWithValue("@PW", password);
                insertCmd.ExecuteNonQuery();

                // Delete pending request
                SqlCommand deleteCmd = new SqlCommand("DELETE FROM PendingAdminRequests WHERE RequestID=@ID", con);
                deleteCmd.Parameters.AddWithValue("@ID", id);
                deleteCmd.ExecuteNonQuery();

                // ⭐ SEND WELCOME EMAIL TO APPROVED ADMIN
                SendWelcomeEmail(email, fullName);

                TempData["success"] = "Admin approved!";
            }

            return RedirectToAction("Dashboard");
        }


        // ===================== DECLINE ======================
        public ActionResult Decline(int id)
        {
            using (SqlConnection con = new SqlConnection(connString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(
                    "DELETE FROM PendingAdminRequests WHERE RequestID=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            TempData["success"] = "Admin Request Declined!";
            return RedirectToAction("Dashboard");
        }

        // ===================== SEND APPROVAL EMAIL ======================
        private void SendApprovalEmail(string email, string fullName)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(email);
                mail.From = new MailAddress("stayeasepgbooking@gmail.com");
                mail.Subject = "StayEase Admin Approval";
                mail.Body = $"Hello {fullName},\n\n" +
                            "Your admin account has been approved! 🎉\n" +
                            "You can now login to StayEase as an Admin.\n\n" +
                            "Thank you,\nStayEase Team";

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential("stayeasepgbooking@gmail.com", "fstz tzcr yxbj bkbx");

                smtp.Send(mail);
            }
            catch
            {
                // ignore
            }
        }



        private void SendWelcomeEmail(string Email, string FullName)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(Email);
                mail.From = new MailAddress("stayeasepgbooking@gmail.com");
                mail.Subject = "Welcome to StayEase!";
                mail.Body = $"Hello {FullName},\n\nWelcome to StayEase! 🎉\nYou registered successfully.\n\nFind your best PG today!\n\nThank you,\nStayEase Team";
                mail.IsBodyHtml = false;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential("stayeasepgbooking@gmail.com", "fstz tzcr yxbj bkbx");
                smtp.EnableSsl = true;
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Welcome email failed: " + ex.Message;
            }
        }

        public ActionResult Verify(int id)
        {
            using (SqlConnection con = new SqlConnection(connString))
            {
                con.Open();

                string email = "";

                // Get admin email
                SqlCommand getCmd = new SqlCommand(
                    "SELECT Email FROM PendingAdminRequests WHERE RequestID=@id", con);
                getCmd.Parameters.AddWithValue("@id", id);
                email = getCmd.ExecuteScalar().ToString();

                // Update status
                SqlCommand updateCmd = new SqlCommand(
                    "UPDATE PendingAdminRequests SET Status='Verified' WHERE RequestID=@id", con);
                updateCmd.Parameters.AddWithValue("@id", id);
                updateCmd.ExecuteNonQuery();

                // Send verification mail
                SendVerifyMail(email);
            }

            TempData["success"] = "Admin verified. Email sent for document submission.";
            return RedirectToAction("Dashboard");
        }


        void SendVerifyMail(string email)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add(email);
            mail.Subject = "Admin Verification – Submit PG Documents";
            mail.Body = @"
             Hello,
             
             Your admin request has been verified.
             
             Please submit the following to proceed:
             • PG details
             • ID proof
             • Property documents
             
             After verification, your account will be approved.
             
             Thank you,
             StayEase Team
             ";

            mail.From = new MailAddress("stayeasepgbooking@gmail.com");

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential("stayeasepgbooking@gmail.com", "fstz tzcr yxbj bkbx");
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }

    }
}
