using System;
using System.Configuration;
using System.Data.SqlClient;
using static System.Net.Mime.MediaTypeNames;

namespace project01
{
    public partial class Site1 : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            
            if (!IsPostBack)
            {
                Label1.Text = Application["LiveUsers"].ToString();

                // 🔹 Hide all placeholders initially
                phGuest.Visible = true;
                phUser.Visible = false;
                phAdmin.Visible = false;

                // 🔹 Check if user is logged in
                if (Session["username"] != null)
                {
                    phGuest.Visible = false;
                    phUser.Visible = true;

                    // 🔹 Load profile image
                    string photoPath = Session["userPhoto"] != null
                        ? Session["userPhoto"].ToString()
                        : "Photos/defaultprofile.jpg";
                    imgProfile.ImageUrl = photoPath;

                    // 🔹 Check user role (admin/user)
                    string role = (Session["role"] != null) ? Session["role"].ToString().Trim().ToLower() : "user";

                    if (role == "admin")
                    {
                        // ✅ Show Admin panel
                        phAdmin.Visible = true;
                        phGuest.Visible = false;
                        PlaceHolder1.Visible = false;
                    }
                    else
                    {
                        // 🔹 Normal user — hide admin panel
                        phAdmin.Visible = false;
                    }
                }
                else
                {
                    // 🔹 Guest user (not logged in)
                    phGuest.Visible = true;
                    phUser.Visible = false;
                    phAdmin.Visible = false;
                    imgProfile.ImageUrl = "Photos/defaultprofile.jpg";
                }
            }
        }


        string cs = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void TimerNotif_Tick(object sender, EventArgs e)
        {
            LoadNotificationCount();
        }
        private void LoadNotificationCount()
        {
            try
            {
                int userId = 0;
                if (Session["UserID"] != null)
                    userId = Convert.ToInt32(Session["UserID"]);

                int unreadCount = 0;

                using (SqlConnection con = new SqlConnection(cs))
                {
                    string query = "SELECT COUNT(*) FROM Notifications WHERE IsRead = 0 AND (UserID IS NULL OR UserID = @UserID)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    con.Open();
                    unreadCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 🔴 Update badge
                notificationBadge.InnerText = unreadCount.ToString();
                notificationBadge.Style["display"] = (unreadCount > 0) ? "inline-block" : "none";
            }
            catch
            {
                // ignore errors silently
            }
        }

        protected void LogoutBtn_Click(object sender, EventArgs e)
        {
            int userID = 0;

            if (Session["UserID"] != null)
            {
                userID = Convert.ToInt32(Session["UserID"]);
            }
            Application["LiveUsers"] = (int)Application["LiveUsers"] - 1;


            string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();

                // Update the latest login entry for this user with LogoutTime
                string updateLogQuery = @"
            UPDATE UserLogs
            SET LogoutTime = GETDATE(), Activity = 'User logged out'
            WHERE UserID = @UserID
              AND LogoutTime IS NULL"; // only update the current active session

                using (SqlCommand cmd = new SqlCommand(updateLogQuery, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    cmd.ExecuteNonQuery();
                }
            }

            // Clear session
            Session.Abandon();
            Session.Clear();

            // Redirect to homepage or login page
            Response.Redirect("Default.aspx");
        }

    }
}
