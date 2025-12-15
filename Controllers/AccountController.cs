using StayEasePG.Models;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using System.Web.Mvc;
using System.Linq;
using System.Text.RegularExpressions;

namespace StayEasePG.Controllers
{
    public class AccountController : Controller
    {
        // DB Connection
        string connString = ConfigurationManager.ConnectionStrings["StayEasePGConn"].ConnectionString;

        // ===================== REGISTER ======================
        // ===================== REGISTER (GET) ======================
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // ===================== REGISTER (POST) ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(UserModel user)
        {
            try
            {
                // -------------------- 1️⃣ VALIDATIONS --------------------
                // EMAIL
                string email = (user.Email ?? "").Trim();
                if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") || !email.EndsWith(".com"))
                {
                    TempData["error"] = "Invalid Email format. Example: name@gmail.com";
                    return View(user);
                }

                // PHONE
                string phone = (user.PhoneNo ?? "").Trim();
                if (phone.Length != 10 || !phone.All(char.IsDigit) ||
                    !(phone.StartsWith("6") || phone.StartsWith("7") || phone.StartsWith("8") || phone.StartsWith("9")))
                {
                    TempData["error"] = "Phone number must be a valid 10-digit Indian number.";
                    return View(user);
                }

                // ID PROOF
                string idType = (user.IDProofType ?? "").Trim();
                string idNumber = (user.IDProofNumber ?? "").Trim();

                if (idType == "Aadhaar")
                {
                    if (idNumber.Length != 12 || !idNumber.All(char.IsDigit))
                    {
                        TempData["error"] = "Aadhaar number must be 12 digits.";
                        return View(user);
                    }
                }
                else if (idType == "PAN")
                {
                    if (!Regex.IsMatch(idNumber, "^[A-Z]{5}[0-9]{4}[A-Z]$"))
                    {
                        TempData["error"] = "PAN number must be in format ABCDE1234F";
                        return View(user);
                    }
                }

                // PASSWORD
                string pass = (user.Password ?? "").Trim();
                if (pass.Length < 6 || pass.Length > 12)
                {
                    TempData["error"] = "Password must be 6–12 characters.";
                    return View(user);
                }
                if (!pass.Any(char.IsUpper))
                {
                    TempData["error"] = "Password must contain at least 1 uppercase letter.";
                    return View(user);
                }
                if (!pass.Any(char.IsDigit))
                {
                    TempData["error"] = "Password must contain at least 1 number.";
                    return View(user);
                }
                if (!pass.Any(ch => "!@#$%^&*()".Contains(ch)))
                {
                    TempData["error"] = "Password must contain at least 1 special character.";
                    return View(user);
                }

                // -------------------- 2️⃣ CHECK DUPLICATE EMAIL --------------------
                string emailLower = email.ToLower();
                string role = (user.Role ?? "").Trim();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // Check Users
                    using (SqlCommand c1 =
                        new SqlCommand("SELECT COUNT(*) FROM Users WHERE LOWER(Email) = @E", con))
                    {
                        c1.Parameters.AddWithValue("@E", emailLower);
                        if ((int)c1.ExecuteScalar() > 0)
                        {
                            TempData["error"] = "Email already exists in Users!";
                            return View(user);
                        }
                    }

                    // Check Admins
                    using (SqlCommand c2 =
                        new SqlCommand("SELECT COUNT(*) FROM Admins WHERE LOWER(Email) = @E", con))
                    {
                        c2.Parameters.AddWithValue("@E", emailLower);
                        if ((int)c2.ExecuteScalar() > 0)
                        {
                            TempData["error"] = "Email already exists in Admins!";
                            return View(user);
                        }
                    }
                }

                // -------------------- 3️⃣ GENERATE OTP --------------------
                Random rnd = new Random();
                int otp = rnd.Next(100000, 999999);

                // save all required details in session
                Session["RegUser"] = user;
                Session["OTP"] = otp;

                // SEND OTP
                SendOTPEmail(emailLower, otp);

                TempData["success"] = "OTP sent to your email!";
                return RedirectToAction("VerifyOTP");
            }
            catch (Exception ex)
            {
                TempData["error"] = "Something went wrong: " + ex.Message;
                return View(user);
            }
        }



        // -------------------- Send registration OTP --------------------
        public bool SendOTPEmail(string email, int otp)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(email);
                mail.From = new MailAddress("stayeasepgbooking@gmail.com");
                mail.Subject = "StayEase Registration OTP";
                mail.Body = $"Your OTP for StayEase registration is: {otp}";
                mail.IsBodyHtml = false;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential("stayeasepgbooking@gmail.com", "fstz tzcr yxbj bkbx");
                smtp.Send(mail);

                return true;
            }
            catch
            {
                return false;
            }
        }

        [HttpGet]
        public ActionResult VerifyOTP()
        {
            return View();
        }

        [HttpPost]
        public ActionResult VerifyOTP(string otp)
        {
            if (Session["OTP"] == null || Session["RegUser"] == null)
            {
                TempData["error"] = "Session expired. Please register again.";
                return RedirectToAction("Register");
            }

            int sessionOtp = Convert.ToInt32(Session["OTP"]);

            if (otp != sessionOtp.ToString())
            {
                TempData["error"] = "Invalid OTP!";
                return View();
            }

            // OTP verified → redirect to CompleteRegister
            TempData["VerifiedUser"] = Session["RegUser"];
            Session.Remove("OTP");

            return RedirectToAction("CompleteRegister");
        }

        public ActionResult CompleteRegister()
        {
            var user = TempData["VerifiedUser"] as UserModel;

            if (user == null)
            {
                TempData["error"] = "Session expired. Please register again.";
                return RedirectToAction("Register");
            }

            string email = user.Email.ToLower();
            bool isAdmin = user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            try
            {
                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    if (isAdmin)
                    {
                        string q = @"INSERT INTO PendingAdminRequests 
          (FullName, Email, PhoneNo, PasswordHash, Status)
          VALUES (@F,@E,@P,@PW,'Pending')";

                        using (SqlCommand cmd = new SqlCommand(q, con))
                        {
                            cmd.Parameters.AddWithValue("@F", user.FullName);
                            cmd.Parameters.AddWithValue("@E", email);
                            cmd.Parameters.AddWithValue("@P", user.PhoneNo);
                            cmd.Parameters.AddWithValue("@PW", user.Password);
                            cmd.ExecuteNonQuery();
                        }

                        TempData["success"] =
                            "✅ OTP verified successfully.\n\n" +
                            "Your Admin registration is submitted.\n" +
                            "After document verification by SuperAdmin, your account will be approved.\n\n" +
                            "You will receive an email once approved.";
                    }

                    else
                    {
                        // Save to Users table
                        string q = @"INSERT INTO Users
                        (FullName, Email, Gender, PhoneNo, Address,
                         IDProofType, IDProofNumber, Occupation, 
                         PasswordHash, Role)
                        VALUES 
                        (@FullName,@Email,@Gender,@PhoneNo,@Address,
                         @IDProofType,@IDProofNumber,@Occupation,
                         @PasswordHash,@Role)";

                        using (SqlCommand cmd = new SqlCommand(q, con))
                        {
                            cmd.Parameters.AddWithValue("@FullName", user.FullName);
                            cmd.Parameters.AddWithValue("@Email", email);
                            cmd.Parameters.AddWithValue("@Gender", user.Gender);
                            cmd.Parameters.AddWithValue("@PhoneNo", user.PhoneNo);
                            cmd.Parameters.AddWithValue("@Address", user.Address);
                            cmd.Parameters.AddWithValue("@IDProofType", user.IDProofType);
                            cmd.Parameters.AddWithValue("@IDProofNumber", user.IDProofNumber);
                            cmd.Parameters.AddWithValue("@Occupation", user.Occupation);
                            cmd.Parameters.AddWithValue("@PasswordHash", user.Password);
                            cmd.Parameters.AddWithValue("@Role", user.Role);
                            cmd.ExecuteNonQuery();
                        }

                        // SEND WELCOME EMAIL FOR USER
                        SendWelcomeEmail(email, user.FullName);
                    }
                }

                Session.Clear();

                TempData["success"] = "Account created successfully!";
                return RedirectToAction("Index", "Home", new { openLogin = "true" });
            }
            catch
            {
                TempData["error"] = "Something went wrong!";
                return RedirectToAction("Register");
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

        // ===================== LOGIN ======================
        [HttpGet]
        public ActionResult Login()
        {
            return RedirectToAction("Index", "Home", new { openLogin = "true" });
        }


        [HttpPost]
        public JsonResult AjaxLogin(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, field = "email", message = "Enter Your ID" });
            }

            if (string.IsNullOrEmpty(password))
            {
                return Json(new { success = false, field = "password", message = "Enter password" });
            }

            string identifier = email.Trim().ToLower();
            string pass = password.Trim();

            using (SqlConnection con = new SqlConnection(connString))
            {
                con.Open();

                // 1) SuperAdmin
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM SuperAdmin WHERE LOWER(Username)=@Id AND PasswordHash=@P", con))
                {
                    cmd.Parameters.AddWithValue("@Id", identifier);
                    cmd.Parameters.AddWithValue("@P", pass);

                    if ((int)cmd.ExecuteScalar() == 1)
                    {
                        Session["SuperAdmin"] = identifier;
                        return Json(new { success = true, redirect = Url.Action("Dashboard", "SuperAdmin") });
                    }
                }

                // 2) Admin
                using (SqlCommand cmd = new SqlCommand("SELECT AdminID, FullName FROM Admins WHERE (LOWER(Email)=@Id OR LOWER(FullName)=@Id) AND PasswordHash=@P", con))
                {
                    cmd.Parameters.AddWithValue("@Id", identifier);
                    cmd.Parameters.AddWithValue("@P", pass);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            Session["UserID"] = dr["AdminID"].ToString();
                            Session["FullName"] = dr["FullName"].ToString();
                            Session["Role"] = "Admin";

                            return Json(new { success = true, redirect = Url.Action("Dashboard", "Admin") });
                        }
                    }
                }

                // 3) User
                using (SqlCommand cmd = new SqlCommand("SELECT UserID, FullName FROM Users WHERE (LOWER(Email)=@Id OR LOWER(FullName)=@Id) AND PasswordHash=@P", con))
                {
                    cmd.Parameters.AddWithValue("@Id", identifier);
                    cmd.Parameters.AddWithValue("@P", pass);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            Session["UserID"] = dr["UserID"].ToString();
                            Session["FullName"] = dr["FullName"].ToString();
                            Session["Role"] = "User";

                            return Json(new { success = true, redirect = Url.Action("Index", "Home") });
                        }
                    }
                }
            }

            // If invalid login
            return Json(new { success = false, field = "login", message = "Invalid email/username or password!" });
        }


        // ===================== LOGOUT ======================
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            TempData["success"] = "Logged out successfully!";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult VerifyRegisterOTP(string Email)
        {
            ViewBag.Email = Email;
            return View();
        }

        [HttpGet]
        public ActionResult ResetPassword(string Email)
        {
            ViewBag.Email = Email;
            return View();
        }

        // ===================== FORGOT PASSWORD ======================
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendOTP(string Email)
        {
            string otp = new Random().Next(100000, 999999).ToString();

            // STEP 1: CHECK EMAIL EXISTS
            using (SqlConnection con = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand("SP_ForgotPassword", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Mode", "CheckEmail");
                cmd.Parameters.AddWithValue("@Email", Email);

                con.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 0)
                {
                    TempData["error"] = "Email not found!";
                    return View("ForgotPassword");
                }
            }

            // STEP 2: SAVE OTP
            using (SqlConnection con = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand("SP_ForgotPassword", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Mode", "InsertOTP");
                cmd.Parameters.AddWithValue("@Email", Email);
                cmd.Parameters.AddWithValue("@OTP", otp);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            // STEP 3: SEND OTP EMAIL
            SendOTPEmailForgotPassword(Email, otp);

            TempData["success"] = $"OTP Sent to {Email}";
            ViewBag.Email = Email;
            return View("VerifyForgotPasswordOTP");
        }

        [HttpPost]
        public ActionResult VerifyForgotPasswordOTP(string Email, string OTP)
        {
            using (SqlConnection con = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand("SP_ForgotPassword", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Mode", "VerifyOTP");
                cmd.Parameters.AddWithValue("@Email", Email);
                cmd.Parameters.AddWithValue("@OTP", OTP);

                con.Open();
                int valid = Convert.ToInt32(cmd.ExecuteScalar());

                if (valid == 1)
                {
                    TempData["success"] = "OTP Verified!";
                    ViewBag.Email = Email;
                    return View("ResetPassword");
                }
                else
                {
                    TempData["error"] = "Invalid OTP!";
                    ViewBag.Email = Email;
                    return View("VerifyForgotPasswordOTP");
                }
            }
        }

        [HttpPost]
        public ActionResult ResetPassword(string Email, string NewPassword, string ConfirmPassword)
        {
            try
            {
                // -------------------- 1️⃣ CHECK EMPTY --------------------
                if (string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    TempData["error"] = "Password cannot be empty.";
                    ViewBag.Email = Email;
                    return View();
                }

                string pass = NewPassword.Trim();

                // -------------------- 2️⃣ PASSWORD VALIDATIONS --------------------
                if (pass.Length < 6 || pass.Length > 12)
                {
                    TempData["error"] = "Password must be 6–12 characters.";
                    ViewBag.Email = Email;
                    return View();
                }

                if (!pass.Any(char.IsUpper))
                {
                    TempData["error"] = "Password must contain at least 1 uppercase letter.";
                    ViewBag.Email = Email;
                    return View();
                }

                if (!pass.Any(char.IsDigit))
                {
                    TempData["error"] = "Password must contain at least 1 number.";
                    ViewBag.Email = Email;
                    return View();
                }

                if (!pass.Any(ch => "!@#$%^&*()".Contains(ch)))
                {
                    TempData["error"] = "Password must contain at least 1 special character.";
                    ViewBag.Email = Email;
                    return View();
                }

                // -------------------- 3️⃣ MATCH PASSWORD --------------------
                if (NewPassword != ConfirmPassword)
                {
                    TempData["error"] = "Password mismatch!";
                    ViewBag.Email = Email;
                    return View();
                }

                // -------------------- 4️⃣ UPDATE PASSWORD IN DB --------------------
                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("SP_ForgotPassword", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Mode", "UpdatePassword");
                    cmd.Parameters.AddWithValue("@Email", Email);
                    cmd.Parameters.AddWithValue("@NewPassword", NewPassword);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                // -------------------- 5️⃣ SUCCESS --------------------
                TempData["success"] = "Password Updated Successfully!";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["error"] = "Error: " + ex.Message;
                ViewBag.Email = Email;
                return View();
            }
        }


        // ===================== EMAIL FUNCTION ======================
        private void SendOTPEmailForgotPassword(string Email, string OTP)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(Email);
                mail.From = new MailAddress("stayeasepgbooking@gmail.com"); // Replace with your email
                mail.Subject = "StayEase - Password Reset OTP";
                mail.Body = "Your OTP is: " + OTP;
                mail.IsBodyHtml = false;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential("stayeasepgbooking@gmail.com", "fstz tzcr yxbj bkbx"); // Use Gmail App Password
                smtp.EnableSsl = true;
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Error sending email: " + ex.Message;
            }
        }
    }
}




//using StayEasePG.Models;



//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Data;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Web;
//using System.Net.Mail;
//using System.Web.Mvc;
//using System.Net;

//namespace StayEasePG.Controllers
//{
//    public class AccountController : Controller
//    {
//        // DB Connection
//        string connString = ConfigurationManager.ConnectionStrings["StayEasePGConn"].ConnectionString;



//        // ===================== REGISTER ======================
//        [HttpGet]
//        public ActionResult Register()
//        {
//            return View(new UserModel());
//        }

//        [HttpPost]
//        public ActionResult Register(UserModel user)
//        {
//            try
//            {
//                // Normalize email (optional but recommended)
//                string email = (user.Email ?? string.Empty).Trim().ToLowerInvariant();
//                string role = (user.Role ?? string.Empty).Trim();

//                using (SqlConnection con = new SqlConnection(connString))
//                {
//                    con.Open();

//                    // Count occurrences in Users and Admins
//                    int userCount = 0, adminCount = 0;

//                    using (SqlCommand cmdUsers = new SqlCommand(
//                        "SELECT COUNT(*) FROM Users WHERE LOWER(Email) = @Email", con))
//                    {
//                        cmdUsers.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = email;
//                        userCount = (int)cmdUsers.ExecuteScalar();
//                    }

//                    using (SqlCommand cmdAdmins = new SqlCommand(
//                        "SELECT COUNT(*) FROM Admins WHERE LOWER(Email) = @Email", con))
//                    {
//                        cmdAdmins.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = email;
//                        adminCount = (int)cmdAdmins.ExecuteScalar();
//                    }

//                    // Enforce per-role rule:
//                    if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
//                    {
//                        // Block if already in Admins
//                        if (adminCount > 0)
//                        {
//                            TempData["error"] = "Email already exists in Admins!";
//                            return View(user);
//                        }
//                        // Allow even if it exists in Users (as long as not in Admins)
//                    }
//                    else // role == "User"
//                    {
//                        // Block if already in Users
//                        if (userCount > 0)
//                        {
//                            TempData["error"] = "Email already exists in Users!";
//                            return View(user);
//                        }
//                        // Allow even if it exists in Admins (as long as not in Users)
//                    }

//                    // Optional: If you want to block after it exists in BOTH tables
//                    // (i.e., prevent a third usage anywhere)
//                    if (userCount > 0 && adminCount > 0)
//                    {
//                        TempData["error"] = "Email has already been used ";
//                        return View(user);
//                    }
//                }

//                // 2. Generate OTP
//                Random rnd = new Random();
//                int otp = rnd.Next(100000, 999999);

//                // 3. Save User Details temporarily in Session
//                Session["Reg_FullName"] = user.FullName;
//                Session["Reg_Email"] = user.Email;
//                Session["Reg_Password"] = user.Password;
//                Session["Reg_Gender"] = user.Gender;
//                Session["Reg_PhoneNo"] = user.PhoneNo;
//                Session["Reg_Address"] = user.Address;
//                Session["Reg_IDProofType"] = user.IDProofType;
//                Session["Reg_IDProofNumber"] = user.IDProofNumber;
//                Session["Reg_Occupation"] = user.Occupation;
//                Session["Reg_Role"] = user.Role;
//                Session["Reg_OTP"] = otp;

//                // 4. Send OTP Mail
//                bool mailStatus = SendOTPEmail(user.Email, otp);
//                if (!mailStatus)
//                {
//                    TempData["error"] = "Failed to send OTP. Try again.";
//                    return View(user);
//                }

//                // 5. Redirect to OTP screen
//                return RedirectToAction("VerifyOTP");
//            }
//            catch
//            {
//                TempData["error"] = "Something went wrong!";
//                return View(user);
//            }
//        }



//        public bool SendOTPEmail(string email, int otp)
//        {
//            try
//            {
//                MailMessage mail = new MailMessage();
//                mail.To.Add(email);
//                mail.From = new MailAddress("stayeasepgbooking@gmail.com");
//                mail.Subject = "StayEase Registration OTP";
//                mail.Body = $"Your OTP for StayEase registration is: {otp}";

//                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
//                smtp.EnableSsl = true;
//                smtp.Credentials = new NetworkCredential("stayeasepgbooking@gmail.com", "fstz tzcr yxbj bkbx");
//                smtp.Send(mail);

//                return true;
//            }
//            catch
//            {
//                return false;
//            }
//        }



//        [HttpGet]
//        public ActionResult VerifyOTP()
//        {
//            return View();
//        }



//        [HttpPost]
//        public ActionResult VerifyOTP(string otp)
//        {
//            // Basic guard
//            if (Session["Reg_OTP"] == null)
//            {
//                TempData["error"] = "Session expired. Please register again.";
//                return RedirectToAction("Register");
//            }

//            int sessionOTP = Convert.ToInt32(Session["Reg_OTP"]);
//            if (!string.Equals(otp, sessionOTP.ToString(), StringComparison.Ordinal))
//            {
//                TempData["error"] = "Invalid OTP!";
//                return View();
//            }

//            // OTP OK → hand off to CompleteRegister with data
//            var user = new UserModel
//            {
//                FullName = Session["Reg_FullName"]?.ToString(),
//                Email = Session["Reg_Email"]?.ToString(),
//                Password = Session["Reg_Password"]?.ToString(), // consider hashing!
//                Gender = Session["Reg_Gender"]?.ToString(),
//                PhoneNo = Session["Reg_PhoneNo"]?.ToString(),
//                Address = Session["Reg_Address"]?.ToString(),
//                IDProofType = Session["Reg_IDProofType"]?.ToString(),
//                IDProofNumber = Session["Reg_IDProofNumber"]?.ToString(),
//                Occupation = Session["Reg_Occupation"]?.ToString(),
//                Role = Session["Reg_Role"]?.ToString()
//            };

//            TempData["UserData"] = user;
//            TempData["Role"] = user.Role;

//            // optionally clear OTP only
//            Session.Remove("Reg_OTP");

//            return RedirectToAction("CompleteRegister");
//        }




//        public ActionResult CompleteRegister(string Email)
//        {
//            var user = TempData["UserData"] as UserModel;
//            var role = (TempData["Role"] as string ?? string.Empty).Trim();

//            if (user == null || string.IsNullOrWhiteSpace(role))
//            {
//                TempData["error"] = "Session expired. Please register again.";
//                return RedirectToAction("Register");
//            }

//            // Normalize email for consistent checks
//            string email = (user.Email ?? string.Empty).Trim().ToLowerInvariant();
//            bool isAdmin = role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

//            try
//            {
//                using (SqlConnection con = new SqlConnection(connString))
//                {
//                    con.Open();

//                    // Count occurrences in both tables (case-insensitive)
//                    int userCount, adminCount;

//                    using (SqlCommand cmdUsers = new SqlCommand(
//                        "SELECT COUNT(*) FROM Users WHERE LOWER(Email) = @Email", con))
//                    {
//                        cmdUsers.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = email;
//                        userCount = (int)cmdUsers.ExecuteScalar();
//                    }

//                    using (SqlCommand cmdAdmins = new SqlCommand(
//                        "SELECT COUNT(*) FROM Admins WHERE LOWER(Email) = @Email", con))
//                    {
//                        cmdAdmins.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = email;
//                        adminCount = (int)cmdAdmins.ExecuteScalar();
//                    }

//                    // Enforce your rules:
//                    if (isAdmin)
//                    {
//                        // Insert into PendingAdminRequests instead of Admins
//                        string query = @"
//        INSERT INTO PendingAdminRequests 
//            (FullName, Email, PhoneNo, PasswordHash)
//        VALUES 
//            (@FullName, @Email, @PhoneNo, @PasswordHash)";

//                        using (SqlCommand cmd = new SqlCommand(query, con))
//                        {
//                            cmd.Parameters.Add("@FullName", SqlDbType.NVarChar).Value = user.FullName ?? "";
//                            cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = email;
//                            cmd.Parameters.Add("@PhoneNo", SqlDbType.NVarChar).Value = user.PhoneNo ?? "";
//                            cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar).Value = user.Password ?? "";
//                            cmd.ExecuteNonQuery();
//                        }

//                        TempData["success"] = "Admin request submitted! Awaiting approval.";
//                    }
//                    else
//                    {
//                        // Block if already in Users
//                        if (userCount > 0)
//                        {
//                            TempData["error"] = "Email already exists in Users!";
//                            return RedirectToAction("Register");
//                        }

//                        // If you want to block a third usage (already in both tables), keep this:
//                        if (userCount > 0 && adminCount > 0)
//                        {
//                            TempData["error"] = "Email has already been used in both Users and Admins!";
//                            return RedirectToAction("Register");
//                        }

//                        // Insert into Users
//                        string query = @"INSERT INTO Users
//                    (FullName, Email, Gender, PhoneNo, Address, IDProofType, IDProofNumber, Occupation, PasswordHash, Role)
//                 VALUES
//                    (@FullName, @Email, @Gender, @PhoneNo, @Address, @IDProofType, @IDProofNumber, @Occupation, @PasswordHash, @Role)";

//                        using (SqlCommand cmd = new SqlCommand(query, con))
//                        {
//                            cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 200).Value = user.FullName ?? "";
//                            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = email;
//                            cmd.Parameters.Add("@Gender", SqlDbType.NVarChar, 50).Value = user.Gender ?? "";
//                            cmd.Parameters.Add("@PhoneNo", SqlDbType.NVarChar, 50).Value = user.PhoneNo ?? "";
//                            cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 500).Value = user.Address ?? "";
//                            cmd.Parameters.Add("@IDProofType", SqlDbType.NVarChar, 50).Value = user.IDProofType ?? "";
//                            cmd.Parameters.Add("@IDProofNumber", SqlDbType.NVarChar, 100).Value = user.IDProofNumber ?? "";
//                            cmd.Parameters.Add("@Occupation", SqlDbType.NVarChar, 100).Value = user.Occupation ?? "";
//                            cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500).Value = user.Password ?? "";
//                            cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 50).Value = role;
//                            cmd.ExecuteNonQuery();
//                        }
//                    }
//                }

//                // Send welcome email (optional)
//                SendWelcomeEmail(email, user.FullName ?? "");

//                // Clean up session after success
//                Session.Clear();

//                TempData["success"] = "Account created successfully!";
//                TempData["OpenLoginPopup"] = "true";
//                return RedirectToAction("Index", "Home", new { openLogin = "true" });
//            }
//            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // unique constraint violation
//            {
//                TempData["error"] = "Email already exists!";
//                return RedirectToAction("Register");
//            }
//            catch
//            {
//                TempData["error"] = "Something went wrong!";
//                return RedirectToAction("Register");
//            }
//        }



//        //private void SendOTPEmail(string Email, string OTP)
//        //{
//        //    try
//        //    {
//        //        MailMessage mail = new MailMessage();
//        //        mail.To.Add(Email);
//        //        mail.From = new MailAddress("stayeasepgbooking@gmail.com");
//        //        mail.Subject = "StayEase Registration OTP";
//        //        mail.Body = $"Your OTP for StayEase registration is: {OTP}";
//        //        mail.IsBodyHtml = false;

//        //        SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
//        //        smtp.Credentials = new NetworkCredential("stayeasepgbooking@gmail.com", "fstz tzcr yxbj bkbx");
//        //        smtp.EnableSsl = true;

//        //        smtp.Send(mail);
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        TempData["error"] = "Email error: " + ex.Message;
//        //    }
//        //}


//        private void SendWelcomeEmail(string Email, string FullName)
//        {
//            try
//            {
//                MailMessage mail = new MailMessage();
//                mail.To.Add(Email);
//                mail.From = new MailAddress("stayeasepgbooking@gmail.com"); // your gmail
//                mail.Subject = "Welcome to StayEase!";
//                mail.Body = $"Hello {FullName},\n\nWelcome to StayEase! 🎉\nYou registered successfully.\n\nFind your best PG today!\n\nThank you,\nStayEase Team";
//                mail.IsBodyHtml = false;

//                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
//                smtp.Credentials = new NetworkCredential("stayeasepgbooking@gmail.com", "fstz tzcr yxbj bkbx"); // your Gmail App Password
//                smtp.EnableSsl = true;

//                smtp.Send(mail);
//            }
//            catch (Exception ex)
//            {
//                TempData["error"] = "Welcome email failed: " + ex.Message;
//            }
//        }



//        // ===================== LOGIN ======================

//        [HttpGet]
//        public ActionResult Login()
//        {
//            return RedirectToAction("Index", "Home", new { openLogin = "true" });
//        }

//        [HttpPost]
//        public ActionResult Login(string Email, string Password)
//        {
//            using (SqlConnection con = new SqlConnection(connString))
//            {
//                con.Open();

//                // 1) SuperAdmin Check
//                SqlCommand cmdSuper = new SqlCommand(
//                    "SELECT COUNT(*) FROM SuperAdmin WHERE LOWER(Username)=@Email AND PasswordHash=@Password", con);

//                cmdSuper.Parameters.AddWithValue("@Email", Email);
//                cmdSuper.Parameters.AddWithValue("@Password", Password);

//                int super = (int)cmdSuper.ExecuteScalar();
//                if (super == 1)
//                {
//                    Session["SuperAdmin"] = Email;
//                    return RedirectToAction("Dashboard", "SuperAdmin");
//                }

//                // 2) Admin Check
//                SqlCommand cmdAdmin = new SqlCommand(
//                    "SELECT COUNT(*) FROM Admins WHERE LOWER(Email)=@Email AND PasswordHash=@Password", con);

//                cmdAdmin.Parameters.AddWithValue("@Email", Email.ToLower());
//                cmdAdmin.Parameters.AddWithValue("@Password", Password);

//                int admin = (int)cmdAdmin.ExecuteScalar();
//                if (admin == 1)
//                {
//                    Session["Admin"] = Email;
//                    return RedirectToAction("Dashboard", "Admin");
//                }

//                // 3) User Check
//                SqlCommand cmdUser = new SqlCommand(
//                    "SELECT COUNT(*) FROM Users WHERE LOWER(Email)=@Email AND PasswordHash=@Password", con);

//                cmdUser.Parameters.AddWithValue("@Email", Email.ToLower());
//                cmdUser.Parameters.AddWithValue("@Password", Password);

//                int user = (int)cmdUser.ExecuteScalar();
//                if (user == 1)
//                {
//                    Session["User"] = Email;
//                    return RedirectToAction("Index", "UserPG");
//                }

//                TempData["error"] = "Invalid email or password!";
//                return RedirectToAction("Index", "Home", new { openLogin = "true" });
//            }
//        }


//        // ===================== LOGOUT ======================
//        public ActionResult Logout()
//        {
//            Session.Clear();
//            Session.Abandon();
//            TempData["success"] = "Logged out successfully!";
//            return RedirectToAction("Login");
//        }

//        [HttpGet]
//        public ActionResult VerifyRegisterOTP(string Email)
//        {
//            ViewBag.Email = Email;
//            return View();
//        }

//        [HttpGet]
//        public ActionResult ResetPassword(string Email)
//        {
//            ViewBag.Email = Email;
//            return View();
//        }

//        // ===================== ======================
//        [HttpGet]
//        public ActionResult ForgotPassword()
//        {
//            return View();
//        }

//        [HttpPost]
//        public ActionResult SendOTP(string Email)
//        {
//            string otp = new Random().Next(100000, 999999).ToString();

//            // STEP 1: CHECK EMAIL EXISTS
//            using (SqlConnection con = new SqlConnection(connString))
//            {
//                SqlCommand cmd = new SqlCommand("SP_ForgotPassword", con);
//                cmd.CommandType = CommandType.StoredProcedure;
//                cmd.Parameters.AddWithValue("@Mode", "CheckEmail");
//                cmd.Parameters.AddWithValue("@Email", Email);

//                con.Open();
//                int count = Convert.ToInt32(cmd.ExecuteScalar());
//                if (count == 0)
//                {
//                    TempData["error"] = "Email not found!";
//                    return View("ForgotPassword");
//                }
//            }

//            // STEP 2: SAVE OTP
//            using (SqlConnection con = new SqlConnection(connString))
//            {
//                SqlCommand cmd = new SqlCommand("SP_ForgotPassword", con);
//                cmd.CommandType = CommandType.StoredProcedure;
//                cmd.Parameters.AddWithValue("@Mode", "InsertOTP");
//                cmd.Parameters.AddWithValue("@Email", Email);
//                cmd.Parameters.AddWithValue("@OTP", otp);

//                con.Open();
//                cmd.ExecuteNonQuery();
//            }

//            // STEP 3: SEND OTP EMAIL
//            SendOTPEmailForgotPassword(Email, otp);

//            TempData["success"] = $"OTP Sent to {Email}";
//            ViewBag.Email = Email;
//            return View("VerifyForgotPasswordOTP");
//        }

//        [HttpPost]
//        public ActionResult VerifyForgotPasswordOTP(string Email, string OTP)
//        {
//            using (SqlConnection con = new SqlConnection(connString))
//            {
//                SqlCommand cmd = new SqlCommand("SP_ForgotPassword", con);
//                cmd.CommandType = CommandType.StoredProcedure;
//                cmd.Parameters.AddWithValue("@Mode", "VerifyOTP");
//                cmd.Parameters.AddWithValue("@Email", Email);
//                cmd.Parameters.AddWithValue("@OTP", OTP);

//                con.Open();
//                int valid = Convert.ToInt32(cmd.ExecuteScalar());

//                if (valid == 1)
//                {
//                    TempData["success"] = "OTP Verified!";
//                    ViewBag.Email = Email;
//                    return View("ResetPassword");
//                }
//                else
//                {
//                    TempData["error"] = "Invalid OTP!";
//                    ViewBag.Email = Email;
//                    return View("VerifyForgotPasswordOTP");
//                }
//            }
//        }

//        [HttpPost]
//        public ActionResult ResetPassword(string Email, string NewPassword, string ConfirmPassword)
//        {
//            if (NewPassword != ConfirmPassword)
//            {
//                TempData["error"] = "Password mismatch!";
//                ViewBag.Email = Email;
//                return View();
//            }

//            using (SqlConnection con = new SqlConnection(connString))
//            {
//                SqlCommand cmd = new SqlCommand("SP_ForgotPassword", con);
//                cmd.CommandType = CommandType.StoredProcedure;
//                cmd.Parameters.AddWithValue("@Mode", "UpdatePassword");
//                cmd.Parameters.AddWithValue("@Email", Email);
//                cmd.Parameters.AddWithValue("@NewPassword", NewPassword);

//                con.Open();
//                cmd.ExecuteNonQuery();
//            }

//            TempData["success"] = "Password Updated Successfully!";
//            return RedirectToAction("Login");
//        }

//        // ===================== EMAIL FUNCTION ======================
//        private void SendOTPEmailForgotPassword(string Email, string OTP)
//        {
//            try
//            {
//                MailMessage mail = new MailMessage();
//                mail.To.Add(Email);
//                mail.From = new MailAddress("stayeasepgbooking@gmail.com"); // Replace with your email
//                mail.Subject = "StayEase - Password Reset OTP";
//                mail.Body = "Your OTP is: " + OTP;
//                mail.IsBodyHtml = false;

//                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
//                smtp.Credentials = new NetworkCredential("stayeasepgbooking@gmail.com", "fstz tzcr yxbj bkbx"); // Use Gmail App Password
//                smtp.EnableSsl = true;
//                smtp.Send(mail);
//            }
//            catch (Exception ex)
//            {
//                TempData["error"] = "Error sending email: " + ex.Message;
//            }
//        }
//    }
//}
