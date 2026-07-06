using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;

namespace project01
{
    public partial class Notifications : Page
    {
        string cs = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["UserID"] == null)
                    Response.Redirect("Login.aspx");

                LoadNotifications();
            }
        }

        private void LoadNotifications()
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"SELECT NotificationID, Title, Message, URL, CreatedAt 
                         FROM Notifications 
                         WHERE UserID=@UserID AND IsRead=0
                         ORDER BY CreatedAt DESC";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserID", Session["UserID"]);
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvNotifications.DataSource = dt;
                gvNotifications.DataBind();

                // ADD THIS BLOCK:
                // If there are unread notifications on load, tell JavaScript to play sound
                if (dt.Rows.Count > 0)
                {
                    // This calls the JavaScript function defined in the .aspx file
                    ClientScript.RegisterStartupScript(this.GetType(), "PlaySoundOnLoad", "attemptPlaySound();", true);
                }
            }
        }
        protected void gvNotifications_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName == "MarkRead")
            {
                int notifId = Convert.ToInt32(e.CommandArgument);
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string query = "UPDATE Notifications SET IsRead=1 WHERE NotificationID=@ID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ID", notifId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                LoadNotifications();
            }
        }
    }
}
