using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI.WebControls;
using System.Web.Configuration;
using System.Text.RegularExpressions;
using System.Web;
using System.Data;

namespace project01
{
    public partial class WebForm11 : System.Web.UI.Page
    {
        private const string ConnStrName = "ConnectionString";
        private const string SessionKeyFlightId = "BookingFlightId";
        private const string SessionKeyClassId = "BookingClassId";
        private const string SessionKeyPrice = "BookingTotalPrice";
        private const string SessionKeyBookingId = "CurrentBookingId";
        private const string SessionKeyUserId = "UserID";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Username"] == null || Session["role"] == null)
            {
                Response.Redirect("Login.aspx?msg=PleaseLoginFirst");
                return;
            }

            if (!IsPostBack)
            {
                if (int.TryParse(Request.QueryString["flightId"], out int fId) &&
                    int.TryParse(Request.QueryString["classId"], out int cId) &&
                    decimal.TryParse(Request.QueryString["price"], out decimal tPrice) &&
                    Request.QueryString["seats"] != null)
                {
                    Session[SessionKeyFlightId] = fId;
                    Session[SessionKeyClassId] = cId;
                    Session[SessionKeyPrice] = tPrice;

                    string seatsString = HttpUtility.UrlDecode(Request.QueryString["seats"]);
                    hdnSeats.Value = seatsString;

                    string[] seatsArray = seatsString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var passengerList = seatsArray.Select(s => new { SeatNumber = s.Trim() }).ToList();

                    rptPassengers.DataSource = passengerList;
                    rptPassengers.DataBind();
                }
                else
                {
                    Response.Redirect("Flights.aspx");
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (Session[SessionKeyFlightId] == null ||
                Session[SessionKeyClassId] == null ||
                Session[SessionKeyPrice] == null ||
                Session[SessionKeyUserId] == null)
            {
                lblMessage.Text = "Booking session data lost. Start again.";
                return;
            }

            int flightId = (int)Session[SessionKeyFlightId];
            int classId = (int)Session[SessionKeyClassId];
            decimal totalPrice = (decimal)Session[SessionKeyPrice];
            int userId = (int)Session[SessionKeyUserId];

            int totalPassengers = rptPassengers.Items.Count;
            if (totalPassengers == 0)
            {
                lblMessage.Text = "No passengers / seats selected.";
                return;
            }

            decimal pricePerPassenger = Math.Round(totalPrice / totalPassengers, 2);

            List<PassengerData> passengersToSave = new List<PassengerData>();
            bool validationFailed = false;

            foreach (RepeaterItem item in rptPassengers.Items)
            {
                TextBox txtName = (TextBox)item.FindControl("txtName");
                TextBox txtAge = (TextBox)item.FindControl("txtAge");
                DropDownList ddlGender = (DropDownList)item.FindControl("ddlGender");
                TextBox txtAadhaar = (TextBox)item.FindControl("txtAadhaar");
                TextBox txtAddress = (TextBox)item.FindControl("txtAddress");
                TextBox txtOccupation = (TextBox)item.FindControl("txtOccupation");
                HiddenField hdnSeatNumber = (HiddenField)item.FindControl("hdnSeatNumber");

                if (!ValidatePassenger(txtName.Text, txtAge.Text, txtAadhaar.Text, txtAddress.Text, txtOccupation.Text))
                {
                    lblMessage.Text = $"Validation failed for Passenger #{item.ItemIndex + 1}.";
                    validationFailed = true;
                    break;
                }

                passengersToSave.Add(new PassengerData
                {
                    Name = txtName.Text.Trim(),
                    Age = Convert.ToInt32(txtAge.Text.Trim()),
                    Gender = ddlGender.SelectedValue,
                    Aadhaar = txtAadhaar.Text.Trim(),
                    Address = txtAddress.Text.Trim(),
                    Occupation = txtOccupation.Text.Trim(),
                    SeatNumber = hdnSeatNumber.Value.Trim(),
                    Price = pricePerPassenger
                });
            }

            if (validationFailed) return;

            try
            {
                int bookingId = SaveBookingAndPassengerDetails(
                    userId, flightId, classId, passengersToSave, totalPrice, totalPassengers
                );

                Session[SessionKeyBookingId] = bookingId;

                Response.Redirect($"payment.aspx?amount={totalPrice}&flightId={flightId}", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error saving booking: " + ex.Message;
            }
        }

        // ---------------- Helper Methods ----------------

        private bool ValidatePassenger(string name, string ageText, string aadhaar, string address, string occupation)
        {
            if (string.IsNullOrWhiteSpace(name) || !Regex.IsMatch(name.Trim(), @"^[a-zA-Z\s\.]+$")) return false;
            if (!int.TryParse(ageText, out int age) || age <= 0 || age > 120) return false;
            if (string.IsNullOrWhiteSpace(aadhaar) || !Regex.IsMatch(aadhaar.Trim(), @"^\d{12}$")) return false;
            if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(occupation)) return false;

            return true;
        }

        private int SaveBookingAndPassengerDetails(int userId, int flightId, int classId,
            List<PassengerData> passengers, decimal totalPrice, int seatsBooked)
        {
            string connStr = WebConfigurationManager.ConnectionStrings[ConnStrName].ConnectionString;
            int bookingId = 0;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // ➤ Insert Booking
                    string bookingQuery = @"
                        INSERT INTO Bookings (UserID, FlightID, FlightClassID, BookingDate, Status, SeatsBooked, FinalPrice)
                        VALUES (@UserID, @FlightID, @FlightClassID, GETDATE(), 'PendingPayment', @SeatsBooked, @FinalPrice);
                        SELECT SCOPE_IDENTITY();";

                    using (SqlCommand cmdBooking = new SqlCommand(bookingQuery, conn, transaction))
                    {
                        cmdBooking.Parameters.AddWithValue("@UserID", userId);
                        cmdBooking.Parameters.AddWithValue("@FlightID", flightId);
                        cmdBooking.Parameters.AddWithValue("@FlightClassID", classId);
                        cmdBooking.Parameters.AddWithValue("@SeatsBooked", seatsBooked);
                        cmdBooking.Parameters.AddWithValue("@FinalPrice", totalPrice);

                        bookingId = Convert.ToInt32(cmdBooking.ExecuteScalar());
                    }

                    // ➤ Insert Passengers + Insert BookedSeats
                    string passengerQuery = @"
                        INSERT INTO PassengerDetails
                        (BookingID, Name, Age, Gender, AadhaarNumber, Address, Occupation, SeatNumber, Price)
                        VALUES (@BookingID, @Name, @Age, @Gender, @Aadhaar, @Address, @Occupation, @SeatNumber, @Price)";

                    foreach (var p in passengers)
                    {
                        // Insert Passenger
                        using (SqlCommand cmdPassenger = new SqlCommand(passengerQuery, conn, transaction))
                        {
                            cmdPassenger.Parameters.AddWithValue("@BookingID", bookingId);
                            cmdPassenger.Parameters.AddWithValue("@Name", p.Name);
                            cmdPassenger.Parameters.AddWithValue("@Age", p.Age);
                            cmdPassenger.Parameters.AddWithValue("@Gender", p.Gender);
                            cmdPassenger.Parameters.AddWithValue("@Aadhaar", p.Aadhaar);
                            cmdPassenger.Parameters.AddWithValue("@Address", p.Address);
                            cmdPassenger.Parameters.AddWithValue("@Occupation", p.Occupation);
                            cmdPassenger.Parameters.AddWithValue("@SeatNumber", p.SeatNumber);
                            cmdPassenger.Parameters.AddWithValue("@Price", p.Price);

                            cmdPassenger.ExecuteNonQuery();
                        }

                        // Insert into BookedSeats
                        InsertBookedSeat(conn, transaction, bookingId, flightId, classId, p.SeatNumber, p.Price);
                    }

                    transaction.Commit();
                    return bookingId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void InsertBookedSeat(SqlConnection conn, SqlTransaction tr, int bookingId, int flightId, int classId, string seatNumber, decimal seatPrice)
        {
            string query = @"
                INSERT INTO BookedSeats (FlightID, FlightClassID, SeatNumber, BookingID, SeatPrice)
                VALUES (@FlightID, @FlightClassID, @SeatNumber, @BookingID, @SeatPrice)";

            using (SqlCommand cmd = new SqlCommand(query, conn, tr))
            {
                cmd.Parameters.AddWithValue("@FlightID", flightId);
                cmd.Parameters.AddWithValue("@FlightClassID", classId);
                cmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                cmd.Parameters.AddWithValue("@BookingID", bookingId);
                cmd.Parameters.AddWithValue("@SeatPrice", seatPrice);

                cmd.ExecuteNonQuery();
            }
        }
    }

    public class PassengerData
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string Aadhaar { get; set; }
        public string Address { get; set; }
        public string Occupation { get; set; }
        public string SeatNumber { get; set; }
        public decimal Price { get; set; }
    }
}
