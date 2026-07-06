using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Web.UI;
using System.Threading.Tasks;

namespace project01
{
    public partial class WebForm12 : System.Web.UI.Page
    {
        private readonly string ConnStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        private const string SessionKeyRegion = "UserRegionLocation";

        protected async void Page_Load(object sender, EventArgs e)
        {
            // Security check
            if (Session["Username"] == null || Session["role"] == null)
            {
                Response.Redirect("Login.aspx?msg=PleaseLoginFirst");
                return;
            }

            if (!IsPostBack)
            {
                // Ensure Page is configured for async processing
                RegisterAsyncTask(new PageAsyncTask(LoadSuccessInfo));
            }
        }

        // ✨ Load payment success page info
        private async Task LoadSuccessInfo()
        {
            try
            {
                string paymentId = Request.QueryString["payment_id"];
                string bookingIdQS = Request.QueryString["bookingId"];
                string amountQS = Request.QueryString["amount"];

                if (!int.TryParse(bookingIdQS, out int bookingId))
                {
                    lblMessage.Text = "Invalid booking reference.";
                    return;
                }

                lblBookingId.Text = bookingId.ToString();
                lblPaymentId.Text = paymentId ?? "N/A";
                lblDate.Text = DateTime.Now.ToString("f");
                lblStatus.Text = "Confirmed";

                decimal amt = 0;
                if (decimal.TryParse(amountQS, NumberStyles.Any, CultureInfo.InvariantCulture, out amt))
                    lblAmount.Text = amt.ToString("C", CultureInfo.CurrentCulture);
                else
                    lblAmount.Text = "₹0.00";

                // 🔥 UPDATE PAYMENT + INSERT LOCATION
                await UpdateBookingPayment(bookingId, paymentId, amt);
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading details: " + ex.Message;
                // Log the exception here
            }
        }

        // ✨ Update booking and insert payment record
        private async Task UpdateBookingPayment(int bookingId, string razorpayId, decimal amount)
        {
            string ipAddress = GetUserIP();
            string region = await GetCachedUserLocation(ipAddress); // IP lookup

            // MANDATORY CHECK: Ensure IP and Region are not null/empty for DB insertion
            if (string.IsNullOrEmpty(ipAddress) || ipAddress == "Unknown")
            {
                ipAddress = "127.0.0.1"; // Fallback to safe local IP for mandatory field
            }
            if (string.IsNullOrEmpty(region) || region == "Unknown Location")
            {
                // Should not happen with the updated GetCachedUserLocation, but safe fallback
                region = "Default Region (API Failure)";
            }

            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                // 1️⃣ Mark booking as confirmed
                string updateBooking = @"UPDATE Bookings 
                                            SET Status='Confirmed', PaymentDate=GETDATE() 
                                            WHERE BookingID=@BookingID";

                using (SqlCommand cmd = new SqlCommand(updateBooking, conn))
                {
                    cmd.Parameters.AddWithValue("@BookingID", bookingId);
                    cmd.ExecuteNonQuery();
                }

                // 2️⃣ Insert payment
                string insertPayment = @"
                    INSERT INTO Payments 
                    (BookingID, Amount, PaymentMethod, PaymentStatus, RazorpayPaymentID, IPAddress, Region)
                    VALUES 
                    (@BookingID, @Amount, 'Razorpay', 'Completed', @RazorpayID, @IPAddress, @Region);
                    SELECT SCOPE_IDENTITY();
                ";

                int paymentId = 0;

                using (SqlCommand cmd = new SqlCommand(insertPayment, conn))
                {
                    cmd.Parameters.AddWithValue("@BookingID", bookingId);
                    cmd.Parameters.AddWithValue("@Amount", amount);
                    cmd.Parameters.AddWithValue("@RazorpayID", (object)razorpayId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IPAddress", ipAddress);
                    cmd.Parameters.AddWithValue("@Region", region);

                    paymentId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 3️⃣ Insert log
                string insertLog = @"
                    INSERT INTO BookingSuccessLog (BookingID, PaymentID, Amount)
                    VALUES (@BookingID, @PaymentID, @Amount)
                ";

                using (SqlCommand cmd = new SqlCommand(insertLog, conn))
                {
                    cmd.Parameters.AddWithValue("@BookingID", bookingId);
                    cmd.Parameters.AddWithValue("@PaymentID", paymentId);
                    cmd.Parameters.AddWithValue("@Amount", amount);
                    cmd.ExecuteNonQuery();
                }

                // 4️⃣ Generate tickets
                GenerateTickets(conn, bookingId);
            }
        }

        // ✨ Robust Get client IP
        private string GetUserIP()
        {
            string ip;
            // Check for load balancer/proxy header first
            string forwardedFor = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                ip = forwardedFor.Split(',').FirstOrDefault()?.Trim();
            }
            else
            {
                // Fallback to direct client address
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            return ip ?? "Unknown";
        }

        // ✨ Get user location with Session Caching
        private async Task<string> GetCachedUserLocation(string ip)
        {
            // 1. Check Cache
            if (Session[SessionKeyRegion] is string cachedRegion && !string.IsNullOrEmpty(cachedRegion) && cachedRegion != "Unknown Location")
            {
                return cachedRegion;
            }

            // 2. Perform Lookup
            string region = await GetUserLocation(ip);

            // 3. Update Cache if successful
            if (region != "Unknown Location")
            {
                Session[SessionKeyRegion] = region;
            }

            // 4. Return result
            return region;
        }

        // ✨ Get city, state, country from IP (SOLVED: Uses public IP for local testing)

        public async Task<string> GetUserLocation(string ip)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    string url = $"https://ipapi.co/{ip}/json/";

                    string json = await client.GetStringAsync(url);
                    dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                    bool error = data.error != null && data.error == true;
                    bool reserved = data.reserved != null && data.reserved == true;

                    if (error || reserved)
                    {
                        return "Unknown Location";
                    }

                    string city = data.city ?? "";
                    string state = data.region ?? "";
                    string country = data.country_name ?? "";

                    string location = string.Join(", ",
                        new[] { city, state, country }.Where(s => !string.IsNullOrWhiteSpace(s)));

                    return string.IsNullOrEmpty(location) ? "Unknown Location" : location;
                }
            }
            catch
            {
                return "Unknown Location";
            }
        }
        // ✨ Ticket Generation
        private void GenerateTickets(SqlConnection conn, int bookingId)
        {
            string sql = @"
                SELECT B.FlightID, B.FlightClassID, BS.SeatNumber, B.FinalPrice,
                        F.DepartureTime, F.ArrivalTime,
                        F.SourceAirportID, F.DestinationAirportID
                FROM Bookings B
                INNER JOIN Flights F ON B.FlightID = F.FlightID
                INNER JOIN BookedSeats BS ON B.BookingID = BS.BookingID
                WHERE B.BookingID = @BookingID";

            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@BookingID", bookingId);
                new SqlDataAdapter(cmd).Fill(dt);
            }

            foreach (DataRow row in dt.Rows)
            {
                string ticketNumber = "TKT" + DateTime.Now.Ticks.ToString().Substring(8);
                string passengerName = "Passenger";

                string insertTicket = @"
                    INSERT INTO TicketDetails 
                    (BookingID, TicketNumber, FlightID, FlightClassID, PassengerName,
                     Status, DepartureTime, ArrivalTime, SourceAirportID, DestinationAirportID,
                     Stops, Terminal, Gate, FinalPrice)
                    VALUES
                    (@BookingID, @TicketNumber, @FlightID, @FlightClassID, @PassengerName,
                     'Confirmed', @Dep, @Arr, @Source, @Dest, 0, 'T1', 'G5', @Price)";

                using (SqlCommand cmd = new SqlCommand(insertTicket, conn))
                {
                    cmd.Parameters.AddWithValue("@BookingID", bookingId);
                    cmd.Parameters.AddWithValue("@TicketNumber", ticketNumber);
                    cmd.Parameters.AddWithValue("@FlightID", row["FlightID"]);
                    cmd.Parameters.AddWithValue("@FlightClassID", row["FlightClassID"]);
                    cmd.Parameters.AddWithValue("@PassengerName", passengerName);
                    cmd.Parameters.AddWithValue("@Dep", row["DepartureTime"]);
                    cmd.Parameters.AddWithValue("@Arr", row["ArrivalTime"]);
                    cmd.Parameters.AddWithValue("@Source", row["SourceAirportID"]);
                    cmd.Parameters.AddWithValue("@Dest", row["DestinationAirportID"]);
                    cmd.Parameters.AddWithValue("@Price", row["FinalPrice"]);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Navigation
        protected void btnViewBookings_Click(object sender, EventArgs e)
        {
            Response.Redirect("MyBookings.aspx");
        }

        protected void btnHome_Click(object sender, EventArgs e)
        {
            Response.Redirect("default.aspx");
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Response.Redirect("ViewTickets.aspx");
        }
    }
}