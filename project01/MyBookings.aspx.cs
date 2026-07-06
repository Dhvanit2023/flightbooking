using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace project01
{
    public partial class WebForm14 : System.Web.UI.Page
    {
        private const string ConnStrName = "ConnectionString";
        private const string SessionKeyUserId = "UserID";

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔒 STEP 1: Security check before anything else
            if (Session["Username"] == null || Session["role"] == null)
            {
                // Session expired or user not logged in
                Response.Redirect("Login.aspx?msg=PleaseLoginFirst");
                return;
            }
            if (Session[SessionKeyUserId] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadMyBookings();
            }
        }

        // ✅ Load all bookings with pagination support
        private void LoadMyBookings()
        {
            int userId = Convert.ToInt32(Session[SessionKeyUserId]);
            string connStr = ConfigurationManager.ConnectionStrings[ConnStrName].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT 
                        B.BookingID,
                        F.FlightNumber,
                        SA.Name AS SourceAirport,
                        DA.Name AS DestinationAirport,
                        CONVERT(VARCHAR, B.BookingDate, 106) AS BookingDate,
                        B.Status,
                        B.SeatsBooked,
                        B.FinalPrice,
                        ISNULL(P.PaymentStatus, 'Pending') AS PaymentStatus,
                        ISNULL(P.PaymentMethod, '-') AS PaymentMethod
                    FROM Bookings B
                    INNER JOIN Flights F ON B.FlightID = F.FlightID
                    INNER JOIN Airports SA ON F.SourceAirportID = SA.AirportID
                    INNER JOIN Airports DA ON F.DestinationAirportID = DA.AirportID
                    LEFT JOIN Payments P ON B.BookingID = P.BookingID
                    WHERE B.UserID = @UserID
                    ORDER BY B.BookingDate DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        gvBookings.DataSource = dt;
                        gvBookings.DataBind();
                        lblMessage.Text = "";
                        gvBookings.Visible = true;
                    }
                    else
                    {
                        lblMessage.Text = "No bookings found.";
                        gvBookings.Visible = false;
                    }
                }
            }
        }

        // ✅ Handle paging event
        protected void gvBookings_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvBookings.PageIndex = e.NewPageIndex;
            LoadMyBookings(); // reload bookings for the new page index
        }

        // ✅ Handle "ViewTickets" button click
        protected void gvBookings_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewTickets")
            {
                int bookingId = Convert.ToInt32(e.CommandArgument);
                Response.Redirect("ViewTickets.aspx?bookingId=" + bookingId);
            }
        }

        // ✅ Redirect back to home
        protected void btnHome_Click(object sender, EventArgs e)
        {
            Response.Redirect("Default.aspx");
        }
    }
}
