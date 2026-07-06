using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace project01
{
    public partial class WebForm4 : System.Web.UI.Page
    {
        private readonly string connStr = System.Configuration.ConfigurationManager
                                          .ConnectionStrings["ConnectionString"].ConnectionString;
      //  decimal discountAmount1 = 0;
        decimal discountAmount;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadAirports();
                LoadClasses();
                ddlAdults.SelectedIndex = 0;
                ddlChildren.SelectedIndex = 0;
                ddlSeniors.SelectedIndex = 0;
                txtReturnDate.Enabled = false;
                gvFlights.Visible = false;
                LoadRecentSearches();
            }
            if (!IsPostBack)
            {
                txtDepartureDate.Attributes["min"] = DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        // ===================== LOAD AIRPORTS =======================
        private void LoadAirports()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = "SELECT AirportID, Name + ' (' + Code + ')' AS AirportDisplay FROM Airports ORDER BY Name";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlFrom.DataSource = dt;
                ddlFrom.DataTextField = "AirportDisplay";
                ddlFrom.DataValueField = "AirportID";
                ddlFrom.DataBind();
                ddlFrom.Items.Insert(0, new ListItem("-- Select From --", ""));

                ddlTo.DataSource = dt.Copy();
                ddlTo.DataTextField = "AirportDisplay";
                ddlTo.DataValueField = "AirportID";
                ddlTo.DataBind();
                ddlTo.Items.Insert(0, new ListItem("-- Select To --", ""));
            }
        }

        // ===================== LOAD FLIGHT CLASSES =======================
        private void LoadClasses()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = "SELECT DISTINCT ClassName FROM FlightClasses ORDER BY ClassName";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlClass.DataSource = dt;
                ddlClass.DataTextField = "ClassName";
                ddlClass.DataValueField = "ClassName";
                ddlClass.DataBind();
                ddlClass.Items.Insert(0, new ListItem("-- Select Class --", ""));
            }
        }

        // ===================== SEARCH FLIGHTS + APPLY DISCOUNT =======================
        protected void btnSearchFlights_Click(object sender, EventArgs e)
        {
            try
            {
                // VALIDATION
                if (string.IsNullOrEmpty(ddlFrom.SelectedValue) ||
                    string.IsNullOrEmpty(ddlTo.SelectedValue) ||
                    string.IsNullOrEmpty(ddlClass.SelectedValue) ||
                    string.IsNullOrEmpty(txtDepartureDate.Text))
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "alert",
                        "alert('⚠️ Please select route, class and departure date!');", true);
                    return;
                }

                int from = int.Parse(ddlFrom.SelectedValue);
                int to = int.Parse(ddlTo.SelectedValue);
                string className = ddlClass.SelectedValue;

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    // ------------------ FLIGHT SEARCH QUERY --------------------
                    string query = @"
SELECT 
    f.FlightID,
    f.FlightNumber,
    ac.Name AS AirlineName,
    a1.Name + ' (' + a1.Code + ')' AS FromAirport,
    a2.Name + ' (' + a2.Code + ')' AS ToAirport,
    f.DepartureTime,
    f.ArrivalTime,
    fc.FlightClassID,  
    fc.BasePrice,      
    fc.ClassName        
FROM Flights f
INNER JOIN Airports a1 ON f.SourceAirportID = a1.AirportID
INNER JOIN Airports a2 ON f.DestinationAirportID = a2.AirportID
INNER JOIN Aircrafts ac ON f.AircraftID = ac.AircraftID
INNER JOIN FlightClasses fc ON f.FlightID = fc.FlightID
WHERE f.SourceAirportID = @From 
  AND f.DestinationAirportID = @To
  AND fc.ClassName = @ClassName";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@From", from);
                    cmd.Parameters.AddWithValue("@To", to);
                    cmd.Parameters.AddWithValue("@ClassName", className);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        gvFlights.Visible = false;
                        pricePreview2.InnerHtml = "";
                        ClientScript.RegisterStartupScript(this.GetType(), "alert",
                            "alert('🚫 No flights found for selected route and class.');", true);
                        return;
                    }

                    // ======================= LOYALTY DISCOUNT ========================
                    int userId = Convert.ToInt32(Session["UserID"]);
                    int bookingCount = 0;

                    string countQuery = @"
SELECT COUNT(*) 
FROM Bookings 
WHERE UserID = @UID
AND MONTH(BookingDate) = MONTH(GETDATE())
AND YEAR(BookingDate) = YEAR(GETDATE())";

                    SqlCommand countCmd = new SqlCommand(countQuery, con);
                    countCmd.Parameters.AddWithValue("@UID", userId);

                    con.Open();
                    bookingCount = Convert.ToInt32(countCmd.ExecuteScalar());
                    con.Close();

                    // DISCOUNT RULE
                    decimal discountPercent = 0;
                    if (bookingCount >= 2 && bookingCount <= 4) discountPercent = 5;
                    else if (bookingCount >= 5 && bookingCount <= 8) discountPercent = 10;
                    else if (bookingCount > 8) discountPercent = 15;

                    // ======================= PRICE CALCULATION ========================
                    decimal basePrice = Convert.ToDecimal(dt.Rows[0]["BasePrice"]);
                    int adults = int.Parse(ddlAdults.SelectedValue);
                    int children = int.Parse(ddlChildren.SelectedValue);
                    int seniors = int.Parse(ddlSeniors.SelectedValue);

                    decimal passengers = adults + (children * 0.6m) + (seniors * 0.7m);
                    decimal estimated = Math.Round(passengers * basePrice, 2);

                    //decimal discountAmount = Math.Round((estimated * discountPercent) / 100, 2);
                    discountAmount = Math.Round((estimated * discountPercent) / 100, 2);

                    decimal finalPrice = estimated - discountAmount;

                    // Display UI Summary
                    pricePreview2.InnerHtml =
                        $"<b>Base Total:</b> ₹{estimated:N0}<br>" +
                        $"<b>Loyalty Discount ({discountPercent}%):</b> -₹{discountAmount:N0}<br>" +
                        $"<b>Final Price:</b> ₹{finalPrice:N0}";
                    Session["lo_dis"] = discountAmount.ToString();
                  //  discountAmount1 = discountAmount;
                    // Bind results
                    gvFlights.DataSource = dt;
                    gvFlights.DataBind();
                    gvFlights.Visible = true;
                }
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "err",
                    $"alert('Error: {HttpUtility.JavaScriptStringEncode(ex.Message)}');", true);
            }
        }

        // ===================== BOOKING REDIRECT =======================
        protected void gvFlights_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "BookFlight")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);

                DataKey dk = gvFlights.DataKeys[rowIndex];
                if (dk == null) return;

                string flightId = dk.Values["FlightID"].ToString();
                string flightClassId = dk.Values["FlightClassID"].ToString();
                string basePrice = dk.Values["BasePrice"].ToString();
                string className = dk.Values["ClassName"].ToString();
                string from = HttpUtility.UrlEncode(gvFlights.Rows[rowIndex].Cells[1].Text);
                string to = HttpUtility.UrlEncode(gvFlights.Rows[rowIndex].Cells[2].Text);
                string departDate = HttpUtility.UrlEncode(txtDepartureDate.Text);

                string url =
                    $"~/Flight_Booking_Page1.aspx?flightId={flightId}&classId={flightClassId}" +
                    $"&basePrice={basePrice}&className={HttpUtility.UrlEncode(className)}" +
                    $"&from={from}&to={to}&depart={departDate}" +
                    $"&ad={ddlAdults.SelectedValue}&ch={ddlChildren.SelectedValue}&sr={ddlSeniors.SelectedValue}";

                Response.Redirect(url, false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        // ===================== RECENT SEARCHES =======================
        private void LoadRecentSearches()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = @"
SELECT TOP 5 
    b.BookingID,
    f.FlightNumber,
    a1.Name + ' (' + a1.Code + ')' AS FromAirport,
    a2.Name + ' (' + a2.Code + ')' AS ToAirport,
    fc.ClassName,
    b.BookingDate
FROM Bookings b
INNER JOIN Flights f ON b.FlightID = f.FlightID
INNER JOIN Airports a1 ON f.SourceAirportID = a1.AirportID
INNER JOIN Airports a2 ON f.DestinationAirportID = a2.AirportID
INNER JOIN FlightClasses fc ON b.FlightClassID = fc.FlightClassID
ORDER BY b.BookingDate DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptRecentSearches.DataSource = dt;
                rptRecentSearches.DataBind();
            }
        }
    }
}
