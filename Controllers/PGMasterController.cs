using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using StayEasePG.Models;
namespace StayEasePG.Controllers
{
    public class PGMasterController : Controller
    {
        string cs = ConfigurationManager.ConnectionStrings["StayEasePGConn"].ConnectionString;
        private bool IsUserLoggedIn()
        {
            return Session["UserID"] != null;
        }

        // ------------------- PG LIST -----------------------
        public ActionResult Index()
        {
            List<PGModel> list = new List<PGModel>();
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                string q = @"
           SELECT PG.PGID, PG.PGName, PGType, PGCategory, City, State, Landmark,
                  ISNULL(RD.PricePerDay,0) AS PricePerDay,
                  ISNULL(RD.PricePerWeek,0) AS PricePerWeek,
                  ISNULL(RD.PricePerMonth,0) AS PricePerMonth
           FROM PG
           LEFT JOIN RoomDetails RD ON PG.PGID = RD.PGID
           WHERE RD.RoomID = (SELECT MIN(RoomID) FROM RoomDetails WHERE PGID = PG.PGID)
       ";
                SqlCommand cmd = new SqlCommand(q, con);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new PGModel
                    {
                        PGID = Convert.ToInt32(dr["PGID"]),
                        PGName = dr["PGName"].ToString(),
                        PGType = dr["PGType"].ToString(),
                        PGCategory = dr["PGCategory"].ToString(),
                        City = dr["City"].ToString(),
                        State = dr["State"].ToString(),
                        Landmark = dr["Landmark"].ToString(),
                        PricePerDay = Convert.ToDecimal(dr["PricePerDay"]),
                        PricePerWeek = Convert.ToDecimal(dr["PricePerWeek"]),
                        PricePerMonth = Convert.ToDecimal(dr["PricePerMonth"]) // new
                    });
                }
            }
            return View(list);
        }
        // ---------------- PG DETAILS -----------------------
        public ActionResult Details(int id)
        {
            PGModel model = new PGModel();
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                // PG DETAILS
                string q1 = "SELECT * FROM PG WHERE PGID=@id";
                SqlCommand cmd1 = new SqlCommand(q1, con);
                cmd1.Parameters.AddWithValue("@id", id);
                SqlDataReader dr1 = cmd1.ExecuteReader();
                if (dr1.Read())
                {
                    model.PGID = id;
                    model.PGName = dr1["PGName"].ToString();
                    model.PGType = dr1["PGType"].ToString();
                    model.PGCategory = dr1["PGCategory"].ToString();
                    model.Description = dr1["Description"].ToString();
                    model.Address = dr1["Address"].ToString();
                    model.City = dr1["City"].ToString();
                    model.State = dr1["State"].ToString();
                    model.PinCode = dr1["PinCode"].ToString();
                    model.Landmark = dr1["Landmark"].ToString();
                }
                dr1.Close();
                // ----------------- AMENITIES (NEW CODE) -----------------
                model.Amenities = new List<AmenityModel>();
                string qa = @"
     SELECT DISTINCT a.AmenityID, a.AmenityName
     FROM RoomDetails rd
     JOIN RoomAmenities ra ON rd.RoomID = ra.RoomID
     JOIN Amenities a ON ra.AmenityID = a.AmenityID
     WHERE rd.PGID = @id";
                using (SqlCommand cmda = new SqlCommand(qa, con))
                {
                    cmda.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader dra = cmda.ExecuteReader())
                    {
                        while (dra.Read())
                        {
                            model.Amenities.Add(new AmenityModel
                            {
                                AmenityID = Convert.ToInt32(dra["AmenityID"]),
                                AmenityName = dra["AmenityName"].ToString()
                            });
                        }
                    }
                }
                // ROOM DETAILS
                model.Rooms = new List<RoomModel>();
                string q2 = "SELECT * FROM RoomDetails WHERE PGID=@id";
                SqlCommand cmd2 = new SqlCommand(q2, con);
                cmd2.Parameters.AddWithValue("@id", id);
                SqlDataReader dr2 = cmd2.ExecuteReader();
                while (dr2.Read())
                {
                    model.Rooms.Add(new RoomModel
                    {
                        RoomID = Convert.ToInt32(dr2["RoomID"]),
                        RoomType = dr2["RoomType"].ToString(),
                        AvailableRooms = Convert.ToInt32(dr2["AvailableRooms"]),
                        PricePerDay = Convert.ToDecimal(dr2["PricePerDay"]),
                        PricePerWeek = Convert.ToDecimal(dr2["PricePerWeek"]),
                        PricePerMonth = Convert.ToDecimal(dr2["PricePerMonth"])
                    });
                }
                dr2.Close();
                // RULES
                string qr = "SELECT * FROM PG_Rules WHERE PGID = @id";
                SqlCommand cmdr = new SqlCommand(qr, con);
                cmdr.Parameters.AddWithValue("@id", id);
                SqlDataReader drr = cmdr.ExecuteReader();
                if (drr.Read())
                {
                    model.Rules = new RuleModel
                    {
                        CheckInTime = drr["CheckInTime"].ToString(),
                        CheckOutTime = drr["CheckOutTime"].ToString(),
                        Restrictions = drr["Restrictions"].ToString(),
                        VisitorsAllowed = Convert.ToBoolean(drr["VisitorsAllowed"]),
                        GateClosingTime = drr["GateClosingTime"].ToString(),
                        NoticePeriod = drr["NoticePeriod"].ToString()
                    };
                }
                drr.Close();
            }
            return View(model);
        }
        // ---------------- 1. START BOOKING -----------------------
        public ActionResult BookDuration(int pgId, int roomId)
        {
            if (!IsUserLoggedIn())
            {
                TempData["Error"] = "Please login to book a PG.";

                string returnUrl = Url.Action("BookDuration", "PGMaster", new { pgId = pgId, roomId = roomId });
                return RedirectToAction("Login", "Account", new { returnUrl = returnUrl });
            }
            InitialBookingModel model = new InitialBookingModel();
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                // PG + Room Details
                string q = @"
                SELECT
                    p.PGName,
                    p.Address + ', ' + p.City + ', ' + p.State AS Location,
                    p.Address,
                    r.RoomType,
                    r.PricePerDay, r.PricePerWeek, r.PricePerMonth,
                    r.MaintenanceCharges, r.AdvanceAmount, r.DepositAmount
                FROM PG p
                JOIN RoomDetails r ON p.PGID = r.PGID
                WHERE p.PGID = @pgId AND r.RoomID = @roomId";
                using (SqlCommand cmd = new SqlCommand(q, con))
                {
                    cmd.Parameters.AddWithValue("@pgId", pgId);
                    cmd.Parameters.AddWithValue("@roomId", roomId);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            model.PGID = pgId;
                            model.RoomID = roomId;
                            model.PGName = dr["PGName"].ToString();
                            model.Location = dr["Location"].ToString();
                            model.Address = dr["Address"].ToString();
                            model.RoomType = dr["RoomType"].ToString();
                            model.PricePerDay = Convert.ToDecimal(dr["PricePerDay"]);
                            model.PricePerWeek = Convert.ToDecimal(dr["PricePerWeek"]);
                            model.PricePerMonth = Convert.ToDecimal(dr["PricePerMonth"]);
                            model.Maintenance = Convert.ToDecimal(dr["MaintenanceCharges"]);
                            model.Advance = Convert.ToDecimal(dr["AdvanceAmount"]);
                            model.Deposit = Convert.ToDecimal(dr["DepositAmount"]);
                        }
                        else
                        {
                            return HttpNotFound("PG or Room details not found.");
                        }
                    }
                }
                // 🔥 CHECK AVAILABILITY
                string availabilityQuery = "SELECT AvailableRooms FROM RoomDetails WHERE RoomID = @roomId";
                using (SqlCommand check = new SqlCommand(availabilityQuery, con))
                {
                    check.Parameters.AddWithValue("@roomId", roomId);
                    int available = Convert.ToInt32(check.ExecuteScalar());
                    if (available <= 0)
                    {
                        TempData["Error"] = "This room is currently FULL. You cannot book it.";
                        return RedirectToAction("Details", new { id = pgId });
                    }
                }
            }
            return View(model);
        }
        // ---------------- CALCULATE BOOKING -----------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CalculateBooking(InitialBookingModel initialModel)
        {
            // 🔥 DOUBLE CHECK AVAILABILITY (avoid race condition)
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                string q = "SELECT AvailableRooms FROM RoomDetails WHERE RoomID = @roomId";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@roomId", initialModel.RoomID);
                int availableRooms = Convert.ToInt32(cmd.ExecuteScalar());
                if (availableRooms <= 0)
                {
                    TempData["Error"] = "Sorry! This room just got FULL. Choose another room.";
                    return RedirectToAction("Details", new { id = initialModel.PGID });
                }
            }
            // Calculate price
            decimal basePrice = 0;
            switch (initialModel.SelectedBookingType)
            {
                case "Day": basePrice = initialModel.PricePerDay; break;
                case "Week": basePrice = initialModel.PricePerWeek; break;
                case "Month": basePrice = initialModel.PricePerMonth; break;
            }
            decimal totalDue = basePrice + initialModel.Maintenance + initialModel.Advance + initialModel.Deposit;
            BookingModel bookingData = new BookingModel
            {
                PGID = initialModel.PGID,
                RoomID = initialModel.RoomID,
                PGName = initialModel.PGName,
                Location = initialModel.Location,
                RoomType = initialModel.RoomType,
                CheckInDate = initialModel.CheckInDate,
                BookingType = initialModel.SelectedBookingType,
                Duration = 1,
                TotalAmount = totalDue,
                PricePerDay = initialModel.PricePerDay,
                PricePerWeek = initialModel.PricePerWeek,
                PricePerMonth = initialModel.PricePerMonth,
                Maintenance = initialModel.Maintenance,
                Advance = initialModel.Advance,
                Deposit = initialModel.Deposit,
                BookingStatus = "Active"
            };
            TempData["BookingData"] = bookingData;
            return RedirectToAction("PaymentDetails");
        }
        // ---------------- PAYMENT DETAILS -----------------------
        public ActionResult PaymentDetails()
        {
            BookingModel bookingData = TempData["BookingData"] as BookingModel;
            if (bookingData == null)
                return RedirectToAction("Index");
            TempData["BookingData"] = bookingData;
            ViewBag.PaymentOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Credit Card", Text = "Credit Card" },
                new SelectListItem { Value = "GPay", Text = "GPay" },
                new SelectListItem { Value = "PhonePe", Text = "PhonePe" },
                new SelectListItem { Value = "Paytm", Text = "Paytm" },
                new SelectListItem { Value = "Cash on Hand", Text = "Cash on Hand (Pay at Check-in)" }
            };
            return View(new PaymentDetailsModel { BookingData = bookingData });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PaymentDetails(PaymentDetailsModel paymentModel)
        {
            BookingModel bookingData = TempData["BookingData"] as BookingModel;
            if (bookingData == null)
                return RedirectToAction("Index");
            bookingData.PaymentMethod = paymentModel.PaymentMethod;
            TempData["BookingData"] = bookingData;
            return RedirectToAction("ConfirmBooking");
        }
        // ---------------- CONFIRM BOOKING -----------------------
        public ActionResult ConfirmBooking()
        {
            BookingModel bookingData = TempData["BookingData"] as BookingModel;
            if (bookingData == null)
                return RedirectToAction("Index");
            TempData["BookingData"] = bookingData;
            return View(bookingData);
        }
        // ---------------- FINALIZE BOOKING -----------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FinalizeBooking()
        {
            BookingModel model = TempData["BookingData"] as BookingModel;
            // 1. Check if model data is available
            if (model == null)
                return RedirectToAction("Index");
            // Retrieve UserID from session (this is correctly retrieved into 'userId')
            int userId = Convert.ToInt32(Session["UserID"]);
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                // Use a SQL Transaction to ensure all or none of the database operations succeed
                SqlTransaction transaction = con.BeginTransaction();
                try
                {
                    // 🔥 FINAL CHECK - Avoid double booking
                    string qCheck = "SELECT AvailableRooms FROM RoomDetails WHERE RoomID=@roomId";
                    using (SqlCommand checkCmd = new SqlCommand(qCheck, con, transaction))
                    {
                        checkCmd.Parameters.AddWithValue("@roomId", model.RoomID);
                        int available = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (available <= 0)
                        {
                            // Rollback if the room is no longer available
                            transaction.Rollback();
                            TempData["Error"] = "Room is no longer available. Booking failed.";
                            return RedirectToAction("Details", new { id = model.PGID });
                        }
                    }
                    // Determine Payment + Transaction Status
                    string paymentStatus = (model.PaymentMethod == "Cash on Hand") ? "Pending" : "Paid";
                    string txnStatus = (model.PaymentMethod == "Cash on Hand") ? "Pending" : "Success";
                    string txnRef = (model.PaymentMethod == "Cash on Hand") ? "N/A" :
                                             "TXN_" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                    int newBookingID = 0;
                    // INSERT Booking
                    string qInsert = @"
           INSERT INTO Booking(UserID, PGID, RoomID, CheckInDate, BookingType, Duration, TotalAmount, PaymentMethod, BookingStatus, PaymentStatus, CreatedOn)
           OUTPUT INSERTED.BookingID
           VALUES(@UserID, @PGID, @RoomID, @CheckInDate, @BookingType, @Duration, @TotalAmount, @PaymentMethod, @BookingStatus, @PaymentStatus, GETDATE())";
                    using (SqlCommand cmdInsert = new SqlCommand(qInsert, con, transaction))
                    {
                        // FIX: Use the local variable 'userId' retrieved from Session
                        cmdInsert.Parameters.AddWithValue("@UserID", userId);
                        cmdInsert.Parameters.AddWithValue("@PGID", model.PGID);
                        cmdInsert.Parameters.AddWithValue("@RoomID", model.RoomID);
                        cmdInsert.Parameters.AddWithValue("@CheckInDate", model.CheckInDate);
                        cmdInsert.Parameters.AddWithValue("@BookingType", model.BookingType);
                        cmdInsert.Parameters.AddWithValue("@Duration", model.Duration);
                        cmdInsert.Parameters.AddWithValue("@TotalAmount", model.TotalAmount);
                        cmdInsert.Parameters.AddWithValue("@PaymentMethod", model.PaymentMethod);
                        cmdInsert.Parameters.AddWithValue("@BookingStatus", model.BookingStatus);
                        cmdInsert.Parameters.AddWithValue("@PaymentStatus", paymentStatus);
                        newBookingID = (int)cmdInsert.ExecuteScalar();
                    }
                    // INSERT Payment Transaction
                    string qInsertTxn = @"
           INSERT INTO PaymentTransaction(BookingID, PaymentMethod, TransactionStatus, Amount, TransactionReference)
           VALUES(@BookingID, @PaymentMethod, @TxnStatus, @Amount, @Ref)";
                    using (SqlCommand cmdTxn = new SqlCommand(qInsertTxn, con, transaction))
                    {
                        cmdTxn.Parameters.AddWithValue("@BookingID", newBookingID);
                        cmdTxn.Parameters.AddWithValue("@PaymentMethod", model.PaymentMethod);
                        cmdTxn.Parameters.AddWithValue("@Amount", model.TotalAmount);
                        cmdTxn.Parameters.AddWithValue("@TxnStatus", txnStatus);
                        cmdTxn.Parameters.AddWithValue("@Ref", txnRef);
                        cmdTxn.ExecuteNonQuery();
                    }
                    // DECREMENT Available Rooms
                    // Note: This UPDATE statement includes a check 'AvailableRooms > 0'
                    //       for an extra safety measure within the transaction.
                    string qUpdateRooms = "UPDATE RoomDetails SET AvailableRooms = AvailableRooms - 1 WHERE RoomID = @roomId AND AvailableRooms > 0";
                    using (SqlCommand cmdUpdate = new SqlCommand(qUpdateRooms, con, transaction))
                    {
                        cmdUpdate.Parameters.AddWithValue("@roomId", model.RoomID);
                        cmdUpdate.ExecuteNonQuery();
                    }
                    // Commit the transaction only if all steps succeeded
                    transaction.Commit();
                    TempData["Success"] = "Booking completed successfully!"; // Add a success message
                }
                catch (Exception ex)
                {
                    // Rollback the transaction on any error
                    transaction.Rollback();
                    // Log the exception (recommended in production code)
                    TempData["Error"] = "Booking Failed! Unexpected error: " + ex.Message;
                    return RedirectToAction("Details", new { id = model.PGID });
                }
            }
            // Redirect to the confirmation page on success
            return RedirectToAction("BookingConfirmation");
        }
        public ActionResult BookingConfirmation()
        {
            // Ensure this view handles the display of TempData["Success"] or just shows a confirmation page.
            return View();
        }
        // ---------------- CANCEL BOOKING -----------------------
        public ActionResult CancelBooking(int bookingId, int roomId)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();
                try
                {
                    string qCancel = "UPDATE Booking SET BookingStatus = 'Cancelled', PaymentStatus = 'Refunded' WHERE BookingID = @bookingId";
                    SqlCommand cmdCancel = new SqlCommand(qCancel, con, transaction);
                    cmdCancel.Parameters.AddWithValue("@bookingId", bookingId);
                    cmdCancel.ExecuteNonQuery();
                    string qUpdate = "UPDATE RoomDetails SET AvailableRooms = AvailableRooms + 1 WHERE RoomID = @roomId";
                    SqlCommand cmdUpdate = new SqlCommand(qUpdate, con, transaction);
                    cmdUpdate.Parameters.AddWithValue("@roomId", roomId);
                    cmdUpdate.ExecuteNonQuery();
                    string qTxn = "UPDATE PaymentTransaction SET TransactionStatus = 'Refunded' WHERE BookingID = @bookingId";
                    SqlCommand cmdTxn = new SqlCommand(qTxn, con, transaction);
                    cmdTxn.Parameters.AddWithValue("@bookingId", bookingId);
                    cmdTxn.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }
            return RedirectToAction("Index");
        }
    }
}
