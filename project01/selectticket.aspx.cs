using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace project01
{
    public partial class SelectTicket : System.Web.UI.Page // Changed class name to SelectTicket
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        // Seat price multipliers (1.5x = 15% extra)
        private const decimal WINDOW_MULTIPLIER = 1.15m;
        private const decimal INTERMEDIATE_MULTIPLIER = 1.05m;
        private const decimal MIDDLE_MULTIPLIER = 1.00m;

        // NOTE: Ensure your designer file links the seatMap control:
      //  protected global::System.Web.UI.HtmlControls.HtmlGenericControl seatMap;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 🔒 STEP 1: Security check before anything else
                if (Session["Username"] == null || Session["role"] == null)
                {
                    // Session expired or user not logged in
                    Response.Redirect("Login.aspx?msg=PleaseLoginFirst");
                    return;
                }
                // 1. Get essential data from query string
                string flightIdStr = Request.QueryString["flightId"];
                string flightClassIdStr = Request.QueryString["classId"];
                string className = Request.QueryString["className"];
                string basePriceStr = Request.QueryString["basePrice"];
                string totalPassengersStr = Request.QueryString["totalPassengers"];

                // Basic validation
                if (string.IsNullOrEmpty(flightIdStr) || string.IsNullOrEmpty(flightClassIdStr) || string.IsNullOrEmpty(className) || string.IsNullOrEmpty(basePriceStr) || string.IsNullOrEmpty(totalPassengersStr))
                {
                    lblMessage.Text = "Error: Missing flight or passenger details. Please go back.";
                    return;
                }

                int flightId = int.Parse(flightIdStr);
                decimal basePrice = decimal.Parse(basePriceStr);
                int totalPassengers = int.Parse(totalPassengersStr);

                // Store data in HiddenFields and Display Labels
                hfFlightID.Value = flightIdStr;
                hfFlightClassID.Value = flightClassIdStr;
                hfBasePrice.Value = basePriceStr;
                hfPassengerCount.Value = totalPassengersStr;

                lblClass.Text = "Selected Class: " + HttpUtility.UrlDecode(className);
                lblPassengerCount.Text = totalPassengersStr;
                lblBasePriceDisplay.Text = $"₹{basePrice:N2}";

                // 2. Generate the seat map structure and get currently booked seats
                List<string> bookedSeats = GetBookedSeats(flightId);
                GenerateSeatMap(HttpUtility.UrlDecode(className), bookedSeats);

                // 3. APPLY DEFAULT SELECTION LOGIC (PRIORITIZING WINDOW SEATS)
                ApplyDefaultSelection(HttpUtility.UrlDecode(className), totalPassengers, basePrice, bookedSeats);

                // 4. Set initial message for the user
                lblMessage.Text = $"Please select exactly {totalPassengersStr} seat(s). Default Window seats are pre-selected.";
            }
        }

        /// <summary>
        /// Selects the required number of seats (prioritizing Window) and calculates the initial price.
        /// </summary>
        protected void ApplyDefaultSelection(string travelClass, int requiredSeats, decimal basePrice, List<string> bookedSeats)
        {
            List<string> defaultSelectedSeats = new List<string>();
            int windowCount = 0;
            int intermediateCount = 0;
            int middleCount = 0;
            decimal totalFare = 0m;

            // Get all available seats, sorted by preference (Window > Intermediate > Middle)
            List<string> availableSeats = GetAvailableSeats(travelClass, bookedSeats);

            // Select the first 'requiredSeats' from the available list
            foreach (string seat in availableSeats)
            {
                if (defaultSelectedSeats.Count >= requiredSeats) break;

                defaultSelectedSeats.Add(seat);

                // Calculate Fare for this seat
                char col = seat.Last();
                int seatsPerRow = travelClass.Equals("Economy", StringComparison.OrdinalIgnoreCase) ? 6 : 4;
                string seatType = GetSeatType(col, seatsPerRow);

                decimal multiplier = GetSeatMultiplier(seatType);
                totalFare += basePrice * multiplier;

                if (seatType == "window") windowCount++;
                else if (seatType == "intermediate") intermediateCount++;
                else middleCount++;
            }

            // Update Hidden Fields and Display Labels with Default Selection
            hfSelectedSeats.Value = string.Join(",", defaultSelectedSeats);
            hfTotalPrice.Value = totalFare.ToString("F2");
            hfWindowCount.Value = windowCount.ToString();
            hfMiddleCount.Value = middleCount.ToString();
            hfIntermediateCount.Value = intermediateCount.ToString();

            // Update display labels 
            lblSelectedSeats.Text = string.Join(", ", defaultSelectedSeats);
            lblSelectedSeatsCount.Text = defaultSelectedSeats.Count.ToString();
            lblCurrentPrice.Text = $"₹{totalFare:N2}";

            // Enable the button since default selection is valid
            btnContinue.Enabled = true;
        }

        // Helper to get multiplier based on seat type
        private decimal GetSeatMultiplier(string seatType)
        {
            if (seatType == "window") return WINDOW_MULTIPLIER;
            if (seatType == "intermediate") return INTERMEDIATE_MULTIPLIER;
            return MIDDLE_MULTIPLIER;
        }

        /// <summary>
        /// Gets all available seats for the class, sorted by preference.
        /// </summary>
        protected List<string> GetAvailableSeats(string travelClass, List<string> bookedSeats)
        {
            List<string> allSeats = new List<string>();
            int startRow = 1, endRow = 5;
            int seatsPerRow = 4;

            if (travelClass.Equals("Business", StringComparison.OrdinalIgnoreCase))
            {
                startRow = 6; endRow = 10;
                seatsPerRow = 4;
            }
            else if (travelClass.Equals("Economy", StringComparison.OrdinalIgnoreCase))
            {
                startRow = 11; endRow = 25;
                seatsPerRow = 6;
            }

            // 1. Generate all seat numbers for the class
            for (int i = startRow; i <= endRow; i++)
            {
                for (int j = 0; j < seatsPerRow; j++)
                {
                    char col = (char)('A' + j);
                    string seatNumber = $"{i}{col}";
                    allSeats.Add(seatNumber);
                }
            }

            // 2. Filter out booked seats
            List<string> availableSeats = allSeats.Except(bookedSeats, StringComparer.OrdinalIgnoreCase).ToList();

            // 3. Sort available seats by preference (Window > Intermediate > Middle)
            availableSeats.Sort((a, b) =>
            {
                string typeA = GetSeatType(a.Last(), seatsPerRow);
                string typeB = GetSeatType(b.Last(), seatsPerRow);

                // Priority: Window (3) > Intermediate (2) > Middle (1)
                int scoreA = typeA == "window" ? 3 : (typeA == "intermediate" ? 2 : 1);
                int scoreB = typeB == "window" ? 3 : (typeB == "intermediate" ? 2 : 1);

                if (scoreA != scoreB) return scoreB.CompareTo(scoreA); // Sort descending by score

                return string.Compare(a, b, StringComparison.Ordinal); // Sort by seat number as tie-breaker
            });

            return availableSeats;
        }


        /// <summary>
        /// Generates the visual HTML structure of the seat map.
        /// </summary>
        protected void GenerateSeatMap(string travelClass, List<string> bookedSeats)
        {
            int startRow = 1, endRow = 5;
            int seatsPerRow = 4;

            if (travelClass.Equals("Business", StringComparison.OrdinalIgnoreCase))
            {
                startRow = 6; endRow = 10;
                seatsPerRow = 4;
            }
            else if (travelClass.Equals("Economy", StringComparison.OrdinalIgnoreCase))
            {
                startRow = 11; endRow = 25;
                seatsPerRow = 6;
            }

            for (int i = startRow; i <= endRow; i++)
            {
                HtmlGenericControl rowDiv = new HtmlGenericControl("div");
                rowDiv.Attributes["class"] = "row";

                // Add Row Number Label before the seats
                rowDiv.Controls.Add(new LiteralControl($"<div style='width: 30px; text-align: right; color: #3b5998; font-weight: bold;'>{i}</div>"));

                for (int j = 0; j < seatsPerRow; j++)
                {
                    char col = (char)('A' + j);
                    string seatNumber = $"{i}{col}";

                    string seatType = GetSeatType(col, seatsPerRow);
                    string cssClass = "seat";
                    if (bookedSeats.Contains(seatNumber)) cssClass += " booked";

                    // The 'selected' class will be added by JavaScript on page load using the hfSelectedSeats value

                    string seatHtml = $"<div class='{cssClass}' data-seattype='{seatType}'>{seatNumber}</div>";
                    rowDiv.Controls.Add(new LiteralControl(seatHtml));

                    // Add Aisle Space
                    if ((seatsPerRow == 6 && j == 2) || (seatsPerRow == 4 && j == 1))
                    {
                        rowDiv.Controls.Add(new LiteralControl("<div class='aisle-space'></div>"));
                    }
                }

                seatMap.Controls.Add(rowDiv);
            }
        }

        // Helper to determine seat type (Window, Intermediate, Middle)
        private string GetSeatType(char col, int seatsPerRow)
        {
            if (seatsPerRow == 6) // Economy: A B C | D E F
            {
                if (col == 'A' || col == 'F') return "window";
                if (col == 'B' || col == 'E') return "middle";
                if (col == 'C' || col == 'D') return "intermediate";
            }
            else if (seatsPerRow == 4) // Business/First: A B | C D
            {
                if (col == 'A' || col == 'D') return "window";
                if (col == 'B' || col == 'C') return "intermediate";
            }
            return "unknown";
        }

        // Helper to fetch already booked seats from DB
        protected List<string> GetBookedSeats(int flightId)
        {
            List<string> bookedSeats = new List<string>();
            string query = @"SELECT SeatNumber 
                     FROM BookedSeats
                     WHERE FlightID = @FlightID AND IsCancelled = 0";

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@FlightID", flightId);

                try
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bookedSeats.Add(reader["SeatNumber"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("DB Error fetching booked seats: " + ex.Message);
                }
            }
            return bookedSeats;
        }


        /// <summary>
        /// Redirects to the confirmation page with all selected seat and price data.
        /// </summary>
        protected void btnContinue_Click(object sender, EventArgs e)
        {
            // 1. Get the final selected seats and price from Hidden Fields
            string selectedSeats = hfSelectedSeats.Value;
            string totalPrice = hfTotalPrice.Value;
            string flightId = hfFlightID.Value;
            string classId = hfFlightClassID.Value;

            // Get surcharge counts
            string windowCount = hfWindowCount.Value;
            string middleCount = hfMiddleCount.Value;
            string intermediateCount = hfIntermediateCount.Value;
            string basePrice = hfBasePrice.Value;

            // Basic validation
            if (string.IsNullOrEmpty(selectedSeats) || selectedSeats.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length != int.Parse(hfPassengerCount.Value))
            {
                lblMessage.Text = "Error: Please select the required number of seats before continuing.";
                return;
            }

            // 2. Redirect to the confirmation page, passing final data via Query String
            string redirectUrl = $"confirmbooking.aspx?flightId={flightId}&classId={classId}&seats={HttpUtility.UrlEncode(selectedSeats)}&price={totalPrice}&basePrice={basePrice}&window={windowCount}&middle={middleCount}&intermediate={intermediateCount}";

            Response.Redirect(redirectUrl);
        }
    }
}