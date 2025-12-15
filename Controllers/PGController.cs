using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using StayEasePG.Models;
namespace StayEasePG.Controllers
{
    public class PGController : Controller
    {
        string cs = ConfigurationManager.ConnectionStrings["StayEasePGConn"].ConnectionString;
        // ------------------ ADD PG (GET) ------------------
        [HttpGet]
        public ActionResult AddPG()
        {
            return View();
        }
        // ------------------ ADD PG (POST) ------------------
        [HttpPost]
        public ActionResult AddPG(PG model)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string query = @"INSERT INTO PG
                                   (PGName, Email, PGType, PGCategory, Description, Address, City, State, PinCode, Landmark)
                                    VALUES
                                   (@name, @email, @type, @category, @desc, @address, @city, @state, @pincode, @landmark)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@name", model.PGName);
                    cmd.Parameters.AddWithValue("@email", model.Email);
                    cmd.Parameters.AddWithValue("@type", model.PGType);
                    cmd.Parameters.AddWithValue("@category", model.PGCategory);
                    cmd.Parameters.AddWithValue("@desc", model.Description ?? "");
                    cmd.Parameters.AddWithValue("@address", model.Address ?? "");
                    cmd.Parameters.AddWithValue("@city", model.City ?? "");
                    cmd.Parameters.AddWithValue("@state", model.State ?? "");
                    cmd.Parameters.AddWithValue("@pincode", model.PinCode ?? "");
                    cmd.Parameters.AddWithValue("@landmark", model.Landmark ?? "");
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                TempData["msg"] = "PG added successfully!";
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Error: " + ex.Message;
            }
            return RedirectToAction("PGList");
        }
        // ------------------ SHOW PG LIST ------------------
        public ActionResult PGList()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = "SELECT * FROM PG";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.Fill(dt);
            }
            return View(dt);
        }
        // ------------------ EDIT PG (GET) ------------------
        [HttpGet]
        public ActionResult EditPG(int id)
        {
            PG model = new PG();
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = "SELECT * FROM PG WHERE PGID=@id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    model.PGID = Convert.ToInt32(dr["PGID"]);
                    model.PGName = dr["PGName"].ToString();
                    model.Email = dr["Email"].ToString();
                    model.PGType = dr["PGType"].ToString();
                    model.PGCategory = dr["PGCategory"].ToString();
                    model.Description = dr["Description"].ToString();
                    model.Address = dr["Address"].ToString();
                    model.City = dr["City"].ToString();
                    model.State = dr["State"].ToString();
                    model.PinCode = dr["PinCode"].ToString();
                    model.Landmark = dr["Landmark"].ToString();
                }
            }
            return View(model);
        }
        // ------------------ EDIT PG (POST) ------------------
        [HttpPost]
        public ActionResult EditPG(PG model)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string query = @"UPDATE PG SET
                                    PGName=@name,
                                    Email=@email,
                                    PGType=@type,
                                    PGCategory=@category,
                                    Description=@desc,
                                    Address=@address,
                                    City=@city,
                                    State=@state,
                                    PinCode=@pincode,
                                    Landmark=@landmark
                                    WHERE PGID=@id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@name", model.PGName);
                    cmd.Parameters.AddWithValue("@email", model.Email);
                    cmd.Parameters.AddWithValue("@type", model.PGType);
                    cmd.Parameters.AddWithValue("@category", model.PGCategory);
                    cmd.Parameters.AddWithValue("@desc", model.Description ?? "");
                    cmd.Parameters.AddWithValue("@address", model.Address ?? "");
                    cmd.Parameters.AddWithValue("@city", model.City ?? "");
                    cmd.Parameters.AddWithValue("@state", model.State ?? "");
                    cmd.Parameters.AddWithValue("@pincode", model.PinCode ?? "");
                    cmd.Parameters.AddWithValue("@landmark", model.Landmark ?? "");
                    cmd.Parameters.AddWithValue("@id", model.PGID);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                TempData["msg"] = "PG updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Error: " + ex.Message;
            }
            return RedirectToAction("PGList");
        }
        // ------------------ DELETE PG ------------------
        public ActionResult DeletePG(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string query = "DELETE FROM PG WHERE PGID=@id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", id);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                TempData["msg"] = "PG deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Error: " + ex.Message;
            }
            return RedirectToAction("PGList");
        }
    }
}