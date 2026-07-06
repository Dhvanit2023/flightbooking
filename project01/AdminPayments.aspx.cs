using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace project01
{
    public partial class WebForm20 : System.Web.UI.Page
    {
        private readonly string connStr = System.Configuration.ConfigurationManager
            .ConnectionStrings["ConnectionString"].ConnectionString;

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
                LoadPayments();
        }

        // Main function (Search + Filter + Sort)
        private void LoadPayments()
        {
            string searchUser = txtSearchUser.Text.Trim();
            string status = ddlStatusFilter.SelectedValue;
            string sortOrder = ddlSort.SelectedValue; // ASC / DESC

            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT 
                        p.PaymentID,
                        b.BookingID,
                        u.Name AS UserName,
                        p.Amount,
                        p.PaymentMethod,
                        p.PaymentDate,
                        p.PaymentStatus,
                        p.RazorpayPaymentID,
                        p.IPAddress,
                        p.Region
                    FROM Payments p
                    INNER JOIN Bookings b ON p.BookingID = b.BookingID
                    INNER JOIN Users_1 u ON b.UserID = u.UserID
                    WHERE 1 = 1";

                // 🔍 Search by username
                if (!string.IsNullOrEmpty(searchUser))
                    query += " AND u.Name LIKE @Search";

                // Filter by payment status
                if (status != "All")
                    query += " AND p.PaymentStatus = @Status";

                // Sort by date
                query += $" ORDER BY p.PaymentDate {sortOrder}";

                SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrEmpty(searchUser))
                    cmd.Parameters.AddWithValue("@Search", "%" + searchUser + "%");

                if (status != "All")
                    cmd.Parameters.AddWithValue("@Status", status);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvPayments.DataSource = dt;
                gvPayments.DataBind();

                lblMessage.Text = dt.Rows.Count > 0 ?
                    $"Showing {dt.Rows.Count} records." :
                    "No records found.";
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            gvPayments.PageIndex = 0; // reset to page 1
            LoadPayments();
        }

        // ✔ Paging Handler
        protected void gvPayments_PageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvPayments.PageIndex = e.NewPageIndex;
            LoadPayments(); // reload with same filters
        }

        public string GetStatusCss(string status)
        {
            status = status.ToLower();
            if (status == "paid" || status == "success")
                return "status-paid";
            else if (status == "pending")
                return "status-pending";
            else
                return "status-failed";
        }
    }
}
