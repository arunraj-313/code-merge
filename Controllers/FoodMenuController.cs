using StayEasePG.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
namespace StayEasePG.Controllers
{
    public class FoodMenuController : Controller
    {
        string cs = ConfigurationManager.ConnectionStrings["StayEasePGConn"].ConnectionString;
        // ---------------------------------------------------------
        // FOOD MENU LIST
        // ---------------------------------------------------------
        public ActionResult FoodMenuList(int? pgid, int? roomid)
        {
            if (pgid == null || roomid == null)
            {
                return Content("Missing PGID or RoomID");
            }
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"SELECT * FROM FoodMenu
                        WHERE PGID=@pgid AND RoomID=@roomid
                        ORDER BY
                          CASE DayOfWeek
                              WHEN 'Monday' THEN 1 WHEN 'Tuesday' THEN 2 WHEN 'Wednesday' THEN 3
                              WHEN 'Thursday' THEN 4 WHEN 'Friday' THEN 5 WHEN 'Saturday' THEN 6
                              WHEN 'Sunday' THEN 7 END,
                          CASE MealType
                              WHEN 'Breakfast' THEN 1 WHEN 'Lunch' THEN 2 WHEN 'Dinner' THEN 3 END";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@pgid", pgid);
                cmd.Parameters.AddWithValue("@roomid", roomid);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            ViewBag.PGID = pgid;
            ViewBag.RoomID = roomid;
            return View("FoodMenuList", dt);
        }
        // ---------------------------------------------------------
        // ADD MENU (GET)
        // ---------------------------------------------------------
        [HttpGet]
        public ActionResult AddMenu(int pgid, int roomid)
        {
            ViewBag.PGID = pgid;
            ViewBag.RoomID = roomid;
            ViewBag.Days = GetDays();
            ViewBag.Meals = GetMealTypes();
            return View("AddFoodMenu");   // 🔥 FIXED VIEW NAME
        }
        // ---------------------------------------------------------
        // ADD MENU (POST)
        // ---------------------------------------------------------
        [HttpPost]
        public ActionResult AddMenu(FoodMenu model)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string query = @"INSERT INTO FoodMenu
                                    (PGID, RoomID, DayOfWeek, MealType, MenuItems)
                                    VALUES (@pgid, @roomid, @day, @meal, @items)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@pgid", model.PGID);
                    cmd.Parameters.AddWithValue("@roomid", model.RoomID);
                    cmd.Parameters.AddWithValue("@day", model.DayOfWeek);
                    cmd.Parameters.AddWithValue("@meal", model.MealType);
                    cmd.Parameters.AddWithValue("@items", model.MenuItems);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                TempData["msg"] = "Menu added successfully!";
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Error: " + ex.Message;
            }
            return RedirectToAction("FoodMenuList", new { pgid = model.PGID, roomid = model.RoomID });
        }
        // ---------------------------------------------------------
        // EDIT MENU (GET)
        // ---------------------------------------------------------
        [HttpGet]
        public ActionResult EditMenu(int id)
        {
            FoodMenu model = new FoodMenu();
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = "SELECT * FROM FoodMenu WHERE MenuID=@id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    model.MenuID = Convert.ToInt32(dr["MenuID"]);
                    model.PGID = Convert.ToInt32(dr["PGID"]);
                    model.RoomID = Convert.ToInt32(dr["RoomID"]);
                    model.DayOfWeek = dr["DayOfWeek"].ToString();
                    model.MealType = dr["MealType"].ToString();
                    model.MenuItems = dr["MenuItems"].ToString();
                }
            }
            ViewBag.PGID = model.PGID;
            ViewBag.RoomID = model.RoomID;
            ViewBag.Days = GetDays();
            ViewBag.Meals = GetMealTypes();
            return View("EditFoodMenu", model);  // 🔥 FIXED VIEW NAME
        }
        // ---------------------------------------------------------
        // EDIT MENU (POST)
        // ---------------------------------------------------------
        [HttpPost]
        public ActionResult EditMenu(FoodMenu model)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string query = @"UPDATE FoodMenu
                                    SET DayOfWeek=@day, MealType=@meal, MenuItems=@items
                                    WHERE MenuID=@id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@day", model.DayOfWeek);
                    cmd.Parameters.AddWithValue("@meal", model.MealType);
                    cmd.Parameters.AddWithValue("@items", model.MenuItems);
                    cmd.Parameters.AddWithValue("@id", model.MenuID);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                TempData["msg"] = "Menu updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Error: " + ex.Message;
            }
            return RedirectToAction("FoodMenuList", new { pgid = model.PGID, roomid = model.RoomID });
        }
        // ---------------------------------------------------------
        // DELETE MENU
        // ---------------------------------------------------------
        public ActionResult DeleteMenu(int id)
        {
            int pgid = 0, roomid = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string getQuery = "SELECT PGID, RoomID FROM FoodMenu WHERE MenuID=@id";
                    SqlCommand cmdGet = new SqlCommand(getQuery, con);
                    cmdGet.Parameters.AddWithValue("@id", id);
                    con.Open();
                    SqlDataReader dr = cmdGet.ExecuteReader();
                    if (dr.Read())
                    {
                        pgid = Convert.ToInt32(dr["PGID"]);
                        roomid = Convert.ToInt32(dr["RoomID"]);
                    }
                    dr.Close();
                    string delQuery = "DELETE FROM FoodMenu WHERE MenuID=@id";
                    SqlCommand cmdDel = new SqlCommand(delQuery, con);
                    cmdDel.Parameters.AddWithValue("@id", id);
                    cmdDel.ExecuteNonQuery();
                }
                TempData["msg"] = "Menu deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Error: " + ex.Message;
            }
            return RedirectToAction("FoodMenuList", new { pgid = pgid, roomid = roomid });
        }
        // ---------------------------------------------------------
        // HELPERS
        // ---------------------------------------------------------
        private List<string> GetDays()
        {
            return new List<string>
           { "Monday","Tuesday","Wednesday","Thursday","Friday","Saturday","Sunday" };
        }
        private List<string> GetMealTypes()
        {
            return new List<string> { "Breakfast", "Lunch", "Dinner" };
        }
    }
}