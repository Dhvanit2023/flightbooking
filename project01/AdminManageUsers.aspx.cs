using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace project01
{
    public partial class WebForm21 : System.Web.UI.Page
    {
        private readonly string connStr =
            System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Username"] == null || Session["role"] == null)
            {
                Response.Redirect("Login.aspx?msg=PleaseLoginFirst");
                return;
            }

            if (!Session["role"].ToString().Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("AccessDenied.aspx");
                return;
            }

            if (!IsPostBack)
                LoadUsers();
        }

        // LOAD USERS (with optional email filter)
        private void LoadUsers(string emailFilter = "")
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = @"SELECT UserID, Name, Email, Phone, CreatedAt, IsVerified, IsAdmin FROM Users_1";

                if (!string.IsNullOrWhiteSpace(emailFilter))
                    query += " WHERE Email LIKE @Email";

                query += " ORDER BY CreatedAt DESC";

                SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrWhiteSpace(emailFilter))
                    cmd.Parameters.AddWithValue("@Email", "%" + emailFilter + "%");

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // STORE for paging
                ViewState["UsersTable"] = dt;

                gvUsers.DataSource = dt;
                gvUsers.DataBind();

                lblMessage.Text = dt.Rows.Count > 0
                    ? $"✅ Showing {dt.Rows.Count} user(s)."
                    : "⚠ No user found.";
            }
        }

        // SEARCH BUTTON
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadUsers(txtSearchEmail.Text.Trim());
        }

        // SHOW ALL
        protected void btnShowAll_Click(object sender, EventArgs e)
        {
            txtSearchEmail.Text = "";
            LoadUsers();
        }

        // GRIDVIEW PAGING
        protected void gvUsers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvUsers.PageIndex = e.NewPageIndex;
            gvUsers.DataSource = ViewState["UsersTable"];
            gvUsers.DataBind();
        }

        // GRIDVIEW COMMANDS (verify/admin/delete)
        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int userId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "ToggleVerify")
                ToggleVerify(userId);

            if (e.CommandName == "ToggleAdmin")
                ToggleAdmin(userId);

            if (e.CommandName == "DeleteUser")
                DeleteUser(userId);

            LoadUsers(txtSearchEmail.Text.Trim());
        }

        private void ToggleVerify(int userId)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Users_1 SET IsVerified = CASE WHEN IsVerified = 1 THEN 0 ELSE 1 END WHERE UserID=@UID", con);
                cmd.Parameters.AddWithValue("@UID", userId);
                cmd.ExecuteNonQuery();
            }
            lblMessage.Text = "✅ User verification status updated!";
        }

        private void ToggleAdmin(int userId)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Users_1 SET IsAdmin = CASE WHEN IsAdmin = 1 THEN 0 ELSE 1 END WHERE UserID=@UID", con);
                cmd.Parameters.AddWithValue("@UID", userId);
                cmd.ExecuteNonQuery();
            }
            lblMessage.Text = "✅ Admin role changed!";
        }

        private void DeleteUser(int userId)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM Users_1 WHERE UserID=@UID", con);
                cmd.Parameters.AddWithValue("@UID", userId);
                cmd.ExecuteNonQuery();
            }
            lblMessage.Text = "🗑️ User deleted!";
        }
    }
}
