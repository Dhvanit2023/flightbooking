using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace project01
{
    public partial class WebForm22 : System.Web.UI.Page
    {
        private readonly string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // SECURITY CHECK
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
                LoadFlights();
        }

        private void LoadFlights()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string q = "SELECT FlightID, FlightNumber FROM Flights ORDER BY FlightNumber";
                SqlDataAdapter da = new SqlDataAdapter(q, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlFlight.DataSource = dt;
                ddlFlight.DataTextField = "FlightNumber";
                ddlFlight.DataValueField = "FlightID";
                ddlFlight.DataBind();
                ddlFlight.Items.Insert(0, new ListItem("-- Select Flight --", ""));
            }
        }

        protected void btnViewStats_Click(object sender, EventArgs e)
        {
            if (ddlFlight.SelectedValue == "")
            {
                lblMessage.Text = "⚠ Please select a flight number!";
                pnlStats.Visible = false;
                return;
            }

            int flightId = Convert.ToInt32(ddlFlight.SelectedValue);
            LoadFlightDetails(flightId);
            LoadBookingStats(flightId);
        }

        private void LoadFlightDetails(int flightId)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string q = @"
                    SELECT F.FlightNumber,
                           A1.Name + ' (' + A1.Code + ')' AS Source,
                           A2.Name + ' (' + A2.Code + ')' AS Destination,
                           F.DepartureTime, F.ArrivalTime,
                           AC.TotalSeats
                    FROM Flights F
                    INNER JOIN Airports A1 ON F.SourceAirportID = A1.AirportID
                    INNER JOIN Airports A2 ON F.DestinationAirportID = A2.AirportID
                    INNER JOIN Aircrafts AC ON F.AircraftID = AC.AircraftID
                    WHERE F.FlightID = @ID";

                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@ID", flightId);
                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    lblFlightNumber.Text = dr["FlightNumber"].ToString();
                    lblRoute.Text = dr["Source"] + " → " + dr["Destination"];
                    lblDeparture.Text = Convert.ToDateTime(dr["DepartureTime"]).ToString("dd-MMM-yyyy HH:mm");
                    lblArrival.Text = Convert.ToDateTime(dr["ArrivalTime"]).ToString("dd-MMM-yyyy HH:mm");
                    lblTotalSeats.Text = dr["TotalSeats"].ToString();
                }
                dr.Close();
            }
        }

        private void LoadBookingStats(int flightId)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string q = @"
                SELECT B.BookingID, U.Name AS UserName, B.SeatsBooked, B.Status, B.BookingDate
                FROM Bookings B
                INNER JOIN Users_1 U ON B.UserID = U.UserID
                WHERE B.FlightID = @FlightID";

                SqlDataAdapter da = new SqlDataAdapter(q, con);
                da.SelectCommand.Parameters.AddWithValue("@FlightID", flightId);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // SAVE TABLE IN VIEWSTATE FOR PAGING
                ViewState["BookingTable"] = dt;

                gvBookings.DataSource = dt;
                gvBookings.DataBind();

                CalculateStats(dt);

                pnlStats.Visible = true;
                lblMessage.Text = "";
            }
        }

        private void CalculateStats(DataTable dt)
        {
            int booked = 0, confirmed = 0, cancelled = 0;

            foreach (DataRow r in dt.Rows)
            {
                int seats = Convert.ToInt32(r["SeatsBooked"]);
                string status = r["Status"].ToString();

                if (status == "Pending" || status == "Paid" || status == "Confirmed")
                    booked += seats;

                if (status == "Confirmed" || status == "Paid")
                    confirmed += seats;

                if (status == "Cancelled")
                    cancelled += seats;
            }

            int totalSeats = Convert.ToInt32(lblTotalSeats.Text);
            int available = totalSeats - booked;

            lblBookedSeats.Text = booked.ToString();
            lblConfirmedSeats.Text = confirmed.ToString();
            lblCancelledSeats.Text = cancelled.ToString();
            lblAvailableSeats.Text = available.ToString();
        }

        // ---------------------- PAGINATION ----------------------
        protected void gvBookings_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvBookings.PageIndex = e.NewPageIndex;
            gvBookings.DataSource = ViewState["BookingTable"];
            gvBookings.DataBind();
        }
    }
}
