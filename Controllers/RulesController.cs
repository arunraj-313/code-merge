using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using StayEasePG.Models;
namespace StayEasePG.Controllers
{
    public class RulesController : Controller
    {
        // Connection string from Web.config
        private string connectionString = ConfigurationManager.ConnectionStrings["StayEasePGConn"].ConnectionString;
        // GET: RulesList
        public ActionResult RulesList()
        {
            List<PG_Rules> rulesList = new List<PG_Rules>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                   SELECT r.RuleID, r.PGID, r.RoomID, r.CheckInTime, r.CheckOutTime, r.Restrictions, r.VisitorsAllowed, r.GateClosingTime, r.NoticePeriod,
                          p.PGName, rd.RoomType
                   FROM PG_Rules r
                   INNER JOIN PG p ON r.PGID = p.PGID
                   INNER JOIN RoomDetails rd ON r.RoomID = rd.RoomID
                   ORDER BY r.RuleID";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rulesList.Add(new PG_Rules
                    {
                        RuleID = Convert.ToInt32(reader["RuleID"]),
                        PGID = Convert.ToInt32(reader["PGID"]),
                        RoomID = Convert.ToInt32(reader["RoomID"]),
                        CheckInTime = reader["CheckInTime"].ToString(),
                        CheckOutTime = reader["CheckOutTime"].ToString(),
                        Restrictions = reader["Restrictions"] == DBNull.Value ? "" : reader["Restrictions"].ToString(),
                        VisitorsAllowed = Convert.ToBoolean(reader["VisitorsAllowed"]),
                        GateClosingTime = reader["GateClosingTime"] == DBNull.Value ? "" : reader["GateClosingTime"].ToString(),
                        NoticePeriod = reader["NoticePeriod"] == DBNull.Value ? "" : reader["NoticePeriod"].ToString(),
                        PG = new PG { PGName = reader["PGName"].ToString() },
                        RoomDetails = new RoomDetails { RoomType = reader["RoomType"].ToString() }
                    });
                }
                con.Close();
            }
            return View(rulesList);
        }
        // GET: AddRules
        public ActionResult AddRules()
        {
            ViewBag.PGID = GetPGList();
            ViewBag.RoomID = GetRoomList();
            return View();
        }
        // POST: AddRules
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRules(PG_Rules rule)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                   INSERT INTO PG_Rules (PGID, RoomID, CheckInTime, CheckOutTime, Restrictions, VisitorsAllowed, GateClosingTime, NoticePeriod)
                   VALUES (@PGID, @RoomID, @CheckInTime, @CheckOutTime, @Restrictions, @VisitorsAllowed, @GateClosingTime, @NoticePeriod)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PGID", rule.PGID);
                cmd.Parameters.AddWithValue("@RoomID", rule.RoomID);
                cmd.Parameters.AddWithValue("@CheckInTime", rule.CheckInTime);
                cmd.Parameters.AddWithValue("@CheckOutTime", rule.CheckOutTime);
                cmd.Parameters.AddWithValue("@Restrictions", (object)rule.Restrictions ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@VisitorsAllowed", rule.VisitorsAllowed);
                cmd.Parameters.AddWithValue("@GateClosingTime", (object)rule.GateClosingTime ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NoticePeriod", (object)rule.NoticePeriod ?? DBNull.Value);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            return RedirectToAction("RulesList");
        }
        // GET: EditRules/5
        public ActionResult EditRules(int id)
        {
            PG_Rules rule = null;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT * FROM PG_Rules WHERE RuleID=@RuleID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@RuleID", id);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    rule = new PG_Rules
                    {
                        RuleID = Convert.ToInt32(reader["RuleID"]),
                        PGID = Convert.ToInt32(reader["PGID"]),
                        RoomID = Convert.ToInt32(reader["RoomID"]),
                        CheckInTime = reader["CheckInTime"].ToString(),
                        CheckOutTime = reader["CheckOutTime"].ToString(),
                        Restrictions = reader["Restrictions"] == DBNull.Value ? "" : reader["Restrictions"].ToString(),
                        VisitorsAllowed = Convert.ToBoolean(reader["VisitorsAllowed"]),
                        GateClosingTime = reader["GateClosingTime"] == DBNull.Value ? "" : reader["GateClosingTime"].ToString(),
                        NoticePeriod = reader["NoticePeriod"] == DBNull.Value ? "" : reader["NoticePeriod"].ToString()
                    };
                }
                con.Close();
            }
            ViewBag.PGID = GetPGList();
            ViewBag.RoomID = GetRoomList();
            return View(rule);
        }
        // POST: EditRules/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRules(PG_Rules rule)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                   UPDATE PG_Rules
                   SET PGID=@PGID, RoomID=@RoomID, CheckInTime=@CheckInTime, CheckOutTime=@CheckOutTime,
                       Restrictions=@Restrictions, VisitorsAllowed=@VisitorsAllowed, GateClosingTime=@GateClosingTime,
                       NoticePeriod=@NoticePeriod
                   WHERE RuleID=@RuleID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PGID", rule.PGID);
                cmd.Parameters.AddWithValue("@RoomID", rule.RoomID);
                cmd.Parameters.AddWithValue("@CheckInTime", rule.CheckInTime);
                cmd.Parameters.AddWithValue("@CheckOutTime", rule.CheckOutTime);
                cmd.Parameters.AddWithValue("@Restrictions", (object)rule.Restrictions ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@VisitorsAllowed", rule.VisitorsAllowed);
                cmd.Parameters.AddWithValue("@GateClosingTime", (object)rule.GateClosingTime ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NoticePeriod", (object)rule.NoticePeriod ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RuleID", rule.RuleID);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            return RedirectToAction("RulesList");
        }
        // GET: DeleteRules/5
        public ActionResult DeleteRules(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM PG_Rules WHERE RuleID=@RuleID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@RuleID", id);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            return RedirectToAction("RulesList");
        }
        // Helper methods for dropdowns
        private List<SelectListItem> GetPGList()
        {
            List<SelectListItem> pgList = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT PGID, PGName FROM PG";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    pgList.Add(new SelectListItem
                    {
                        Value = reader["PGID"].ToString(),
                        Text = reader["PGName"].ToString()
                    });
                }
                con.Close();
            }
            return pgList;
        }
        private List<SelectListItem> GetRoomList()
        {
            List<SelectListItem> roomList = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT RoomID, RoomType FROM RoomDetails";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    roomList.Add(new SelectListItem
                    {
                        Value = reader["RoomID"].ToString(),
                        Text = reader["RoomType"].ToString()
                    });
                }
                con.Close();
            }
            return roomList;
        }
    }
}