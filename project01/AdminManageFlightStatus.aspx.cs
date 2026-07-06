using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace project01
{
    public partial class WebForm19 : System.Web.UI.Page
    {
        private readonly string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Username"] == null || Session["role"] == null)
            {
                Response.Redirect("Login.aspx?msg=PleaseLoginFirst");
                return;
            }

            string role = Session["role"].ToString();
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("AccessDenied.aspx");
                return;
            }

            if (!IsPostBack)
                LoadFlightStatus();
        }

        private void LoadFlightStatus(string flightSearch = "")
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT 
                        f.FlightID,
                        f.FlightNumber,
                        a1.Name + ' (' + a1.Code + ')' AS FromAirport,
                        a2.Name + ' (' + a2.Code + ')' AS ToAirport,
                        f.DepartureTime,
                        f.ArrivalTime,
                        ISNULL(fd.Status, 'On Time') AS Status,
                        ISNULL(fd.Message, '-') AS Message
                    FROM Flights f
                    INNER JOIN Airports a1 ON f.SourceAirportID = a1.AirportID
                    INNER JOIN Airports a2 ON f.DestinationAirportID = a2.AirportID
                    LEFT JOIN FlightDelays fd ON f.FlightID = fd.FlightID";

                if (!string.IsNullOrEmpty(flightSearch))
                    query += " WHERE f.FlightNumber LIKE @Search";

                query += " ORDER BY f.DepartureTime DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                if (!string.IsNullOrEmpty(flightSearch))
                    cmd.Parameters.AddWithValue("@Search", "%" + flightSearch + "%");

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvFlightStatus.DataSource = dt;
                gvFlightStatus.DataBind();

                lblMessage.Text = dt.Rows.Count > 0
                    ? $"✅ Showing {dt.Rows.Count} flight(s)."
                    : "⚠ No flight found.";
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvFlightStatus.EditIndex = -1;
            LoadFlightStatus(txtSearchFlight.Text.Trim());
        }

        protected void btnShowAll_Click(object sender, EventArgs e)
        {
            txtSearchFlight.Text = "";
            gvFlightStatus.EditIndex = -1;
            LoadFlightStatus();
        }

        protected void gvFlightStatus_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvFlightStatus.PageIndex = e.NewPageIndex;
            LoadFlightStatus(txtSearchFlight.Text.Trim());
        }

        protected void gvFlightStatus_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvFlightStatus.EditIndex = e.NewEditIndex;
            LoadFlightStatus(txtSearchFlight.Text.Trim());
        }

        protected void gvFlightStatus_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvFlightStatus.EditIndex = -1;
            LoadFlightStatus(txtSearchFlight.Text.Trim());
        }

        protected void gvFlightStatus_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = gvFlightStatus.Rows[e.RowIndex];
            int flightId = Convert.ToInt32(gvFlightStatus.DataKeys[e.RowIndex].Value);

            DropDownList ddlStatus = (DropDownList)row.FindControl("ddlStatus");
            TextBox txtMessage = (TextBox)row.FindControl("txtMessage");

            string status = ddlStatus?.SelectedValue ?? "On Time";
            string message = txtMessage?.Text.Trim() ?? "-";

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();

                SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM FlightDelays WHERE FlightID = @FlightID", con);
                checkCmd.Parameters.AddWithValue("@FlightID", flightId);
                int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                SqlCommand cmd;
                if (count > 0)
                {
                    cmd = new SqlCommand("UPDATE FlightDelays SET Status=@Status, Message=@Message, UpdatedAt=GETDATE() WHERE FlightID=@FlightID", con);
                }
                else
                {
                    cmd = new SqlCommand("INSERT INTO FlightDelays (FlightID, Status, Message, UpdatedAt) VALUES (@FlightID, @Status, @Message, GETDATE())", con);
                }

                cmd.Parameters.AddWithValue("@FlightID", flightId);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.ExecuteNonQuery();

                NotifyBookingUsers(flightId, status, message);
            }

            lblMessage.Text = "✅ Flight updated successfully and passengers notified!";
            gvFlightStatus.EditIndex = -1;
            LoadFlightStatus(txtSearchFlight.Text.Trim());
        }

        private void NotifyBookingUsers(int flightId, string status, string message)
        {
            try
            {
                int count = 0;
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();
                    string query = @"
                        SELECT 
                            u.Email, u.Name, b.BookingID, f.FlightNumber, 
                            a1.Name AS Source, a2.Name AS Destination, 
                            f.DepartureTime, f.ArrivalTime
                        FROM Bookings b
                        INNER JOIN Users_1 u ON b.UserID = u.UserID
                        INNER JOIN Flights f ON b.FlightID = f.FlightID
                        INNER JOIN Airports a1 ON f.SourceAirportID = a1.AirportID
                        INNER JOIN Airports a2 ON f.DestinationAirportID = a2.AirportID
                        WHERE b.FlightID = @FlightID AND b.Status IN ('Confirmed', 'Paid', 'Pending')";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@FlightID", flightId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string email = reader["Email"].ToString();
                        string userName = reader["Name"].ToString();
                        string flightNo = reader["FlightNumber"].ToString();
                        string from = reader["Source"].ToString();
                        string to = reader["Destination"].ToString();

                        string subject = $"✈ Flight {flightNo} Status Update: {status}";
                        string body = $@"Dear {userName},<br/><br/>
                            Your flight <b>{flightNo}</b> status has been updated to <b>{status}</b>.<br/>
                            <b>New Status Message:</b> {message}<br/>
                            <b>Route:</b> {from} → {to}<br/>
                            <b>Departure Time:</b> {((DateTime)reader["DepartureTime"]).ToString("dd-MMM HH:mm")}<br/>
                            <b>Arrival Time:</b> {((DateTime)reader["ArrivalTime"]).ToString("dd-MMM HH:mm")}<br/><br/>
                            Please check the latest information on our website.<br/><br/>
                            Thank you,<br/>
                            Flight Booking Team";

                        if (SendEmail(email, subject, body))
                            count++;
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Email sending error: " + ex.Message;
            }
        }

        private bool SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                string fromAddress = "patelkanostudent@gmail.com";
                string password = "xrvx welj nagp bsbz"; // App password - consider storing securely

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromAddress, "Flight Booking Admin");
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(fromAddress, password),
                    EnableSsl = true
                };
                smtp.Send(mail);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Set DropDownList selected value during RowDataBound event for editing row
        protected void gvFlightStatus_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow && gvFlightStatus.EditIndex == e.Row.RowIndex)
            {
                DropDownList ddlStatus = (DropDownList)e.Row.FindControl("ddlStatus");
                if (ddlStatus != null)
                {
                    string currentStatus = DataBinder.Eval(e.Row.DataItem, "Status")?.ToString();
                    if (!string.IsNullOrEmpty(currentStatus))
                    {
                        ListItem item = ddlStatus.Items.FindByText(currentStatus);
                        if (item != null)
                        {
                            ddlStatus.ClearSelection();
                            item.Selected = true;
                        }
                    }
                }
            }
        }
    }
}
