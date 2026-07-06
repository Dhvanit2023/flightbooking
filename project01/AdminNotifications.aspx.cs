using Microsoft.AspNet.SignalR;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace project01
{
    public partial class WebForm10 : Page
    {
        string cs = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔒 STEP 1: Security check before anything else
            if (Session["Username"] == null || Session["role"] == null)
            {
                // Session expired or user not logged in
                Response.Redirect("Login.aspx?msg=PleaseLoginFirst");
                return;
            }

            // 🔒 STEP 2: Optional — restrict to admin only
            string role = Session["role"].ToString();
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                // Not an admin — block access
                Response.Redirect("AccessDenied.aspx");
                return;
            }
            if (!IsPostBack)
            {
                LoadUsers();
            }
        }

        private void LoadUsers()
        {
            ddlUsers.Items.Clear();
            ddlUsers.Items.Add(new System.Web.UI.WebControls.ListItem("--Select User--", ""));

            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = "SELECT UserID, Username FROM Users_1"; // Make sure this table exists
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string userName = reader["Username"].ToString();
                    string userId = reader["UserID"].ToString();

                    ddlUsers.Items.Add(new System.Web.UI.WebControls.ListItem(userName, userId));
                }
            }

            ddlUsers.Enabled = !chkAllUsers.Checked;
        }

        protected void chkAllUsers_CheckedChanged(object sender, EventArgs e)
        {
            ddlUsers.Enabled = !chkAllUsers.Checked;
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            string title = txtTitle.Text.Trim();
            string message = txtMessage.Text.Trim();
            string url = txtURL.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
            {
                lblMessage.Text = "Title and message are required!";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int? userId = null;

            if (!chkAllUsers.Checked)
            {
                if (!string.IsNullOrEmpty(ddlUsers.SelectedValue))
                {
                    userId = Convert.ToInt32(ddlUsers.SelectedValue);
                }
                else
                {
                    lblMessage.Text = "Please select a user or check 'Send to all users'.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }
            }

            // Insert notification into DB
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = "INSERT INTO Notifications (UserID, Title, Message, URL, CreatedAt) VALUES (@UserID, @Title, @Message, @URL, GETDATE())";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserID", (object)userId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.Parameters.AddWithValue("@URL", url);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            // Send notification via SignalR
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            if (chkAllUsers.Checked)
            {
                context.Clients.All.receiveNotification(title, message, url);
            }
            else
            {
                context.Clients.User(userId.ToString()).receiveNotification(title, message, url);
            }

            lblMessage.Text = "Notification sent successfully!";
            lblMessage.ForeColor = System.Drawing.Color.Green;

            txtTitle.Text = "";
            txtMessage.Text = "";
            txtURL.Text = "";
            ddlUsers.SelectedIndex = 0;
        }
    }
}
