using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;

namespace project01
{
    public partial class AdminUserLogs : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Security check
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
                BindUserLogs();
        }

        private void BindUserLogs()
        {
            string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT ul.LogID, u.Name AS UserName, ul.IPAddress, ul.LoginTime, ul.LogoutTime, ul.Activity
                    FROM UserLogs ul
                    INNER JOIN Users_1 u ON ul.UserID = u.UserID
                    ORDER BY ul.LoginTime DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);

                DataTable dt = new DataTable();
                da.Fill(dt);

                gvUserLogs.DataSource = dt;
                gvUserLogs.DataBind();
            }
        }

        // ⭐ Paging event
        protected void gvUserLogs_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvUserLogs.PageIndex = e.NewPageIndex;
            BindUserLogs();
        }
    }
}
