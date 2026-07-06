using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Web;
using System.Web.UI;

namespace project01
{
    public partial class WebForm15 : System.Web.UI.Page
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
                LoadTicketPage();
            }
        }

        // ===================================
        // LOAD BOOKING, PASSENGER & TICKET
        // ===================================
        private void LoadTicketPage()
        {
            try
            {
                string paymentId = Request.QueryString["payment_id"];
                string bookingIdQS = Request.QueryString["bookingId"];
                string amountQS = Request.QueryString["amount"];
                int bookingId = 0;

                if (!string.IsNullOrEmpty(bookingIdQS) && int.TryParse(bookingIdQS, out int parsed))
                    bookingId = parsed;
                else if (Session["CurrentBookingId"] != null)
                    bookingId = Convert.ToInt32(Session["CurrentBookingId"]);
                else
                {
                    lblMessage.Text = "Booking reference not found.";
                    return;
                }

                lblBookingId.Text = bookingId.ToString();
                lblDate.Text = DateTime.Now.ToString("f");
               // lblStatus.Text = "Confirmed";

                LoadBookingAndPayment(bookingId, paymentId, amountQS);
                LoadPassengerList(bookingId);
                LoadOrGenerateTickets(bookingId);
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading ticket: " + ex.Message;
            }
        }

        private void LoadBookingAndPayment(int bookingId, string paymentIdQS, string amountQS)
        {
            string connStr = ConfigurationManager.ConnectionStrings[ConnStrName].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        B.FinalPrice,
                        P.RazorpayPaymentID,
                        P.Amount,
                        F.FlightNumber,
                        SA.Name AS Source,
                        SA.City AS SourceCity,
                        DA.Name AS Destination,
                        DA.City AS DestinationCity,
                        F.DepartureTime,
                        F.ArrivalTime,
                        (SELECT COUNT(*) FROM PassengerDetails WHERE BookingID = B.BookingID) AS TotalPassengers
                    FROM Bookings B
                    INNER JOIN Flights F ON B.FlightID = F.FlightID
                    INNER JOIN Airports SA ON F.SourceAirportID = SA.AirportID
                    INNER JOIN Airports DA ON F.DestinationAirportID = DA.AirportID
                    LEFT JOIN Payments P ON B.BookingID = P.BookingID
                    WHERE B.BookingID = @BookingID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BookingID", bookingId);
                    SqlDataReader r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        lblPaymentId.Text = r["RazorpayPaymentID"] != DBNull.Value
                            ? r["RazorpayPaymentID"].ToString()
                            : (paymentIdQS ?? "Not Available");

                        decimal amount = r["Amount"] != DBNull.Value
                            ? Convert.ToDecimal(r["Amount"])
                            : Convert.ToDecimal(amountQS ?? "0");

                        lblAmount.Text = amount.ToString("C", CultureInfo.CurrentCulture);
                        lblFrom.Text = r["SourceCity"] + " (" + r["Source"] + ")";
                        lblTo.Text = r["DestinationCity"] + " (" + r["Destination"] + ")";
                        lblFlightNumber.Text = r["FlightNumber"].ToString();
                        lblDeparture.Text = Convert.ToDateTime(r["DepartureTime"]).ToString("f");
                        lblArrival.Text = Convert.ToDateTime(r["ArrivalTime"]).ToString("f");
                        lblPassengers.Text = r["TotalPassengers"].ToString();
                    }
                }
            }
        }

        private void LoadPassengerList(int bookingId)
        {
            string connStr = ConfigurationManager.ConnectionStrings[ConnStrName].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string q = @"SELECT Name AS PassengerName, Age, Gender, SeatNumber, Price 
                             FROM PassengerDetails WHERE BookingID=@BID";
                SqlDataAdapter da = new SqlDataAdapter(q, conn);
                da.SelectCommand.Parameters.AddWithValue("@BID", bookingId);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvPassengers.DataSource = dt;
                gvPassengers.DataBind();

                ViewState["PassengerList"] = dt;
            }
        }

        private void LoadOrGenerateTickets(int bookingId)
        {
            string connStr = ConfigurationManager.ConnectionStrings[ConnStrName].ConnectionString;
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 1) Get status (use ExecuteScalar so no DataReader is left open)
                string checkStatusQuery = "SELECT Status FROM TicketDetails WHERE BookingID=@BookingID";
                using (SqlCommand statusCmd = new SqlCommand(checkStatusQuery, conn))
                {
                    statusCmd.Parameters.AddWithValue("@BookingID", bookingId);
                    object statusObj = statusCmd.ExecuteScalar();
                    if (statusObj != null && statusObj != DBNull.Value)
                    {
                        lblStatus.Text = statusObj.ToString();
                    }
                    else
                    {
                        lblStatus.Text = string.Empty;
                    }
                }

                // 2) Check count (ExecuteScalar)
                string checkCountQuery = "SELECT COUNT(*) FROM TicketDetails WHERE BookingID=@BookingID";
                using (SqlCommand countCmd = new SqlCommand(checkCountQuery, conn))
                {
                    countCmd.Parameters.AddWithValue("@BookingID", bookingId);
                    int count = Convert.ToInt32(countCmd.ExecuteScalar());

                    if (count == 0)
                    {
                        string insertQuery = @"
                    INSERT INTO TicketDetails 
                    (BookingID, TicketNumber, FlightID, FlightClassID, PassengerName, 
                     Status, DepartureTime, ArrivalTime, SourceAirportID, DestinationAirportID, Stops, Terminal, Gate, FinalPrice)
                    SELECT 
                        B.BookingID,
                        'TKT' + CAST(ABS(CHECKSUM(NEWID())) AS VARCHAR(12)),
                        B.FlightID,
                        B.FlightClassID,
                        PD.Name,
                        'Confirmed',
                        F.DepartureTime,
                        F.ArrivalTime,
                        F.SourceAirportID,
                        F.DestinationAirportID,
                        0,
                        'T1',
                        'G5',
                        B.FinalPrice / (SELECT COUNT(*) FROM PassengerDetails WHERE BookingID = B.BookingID)
                    FROM Bookings B
                    INNER JOIN Flights F ON B.FlightID = F.FlightID
                    INNER JOIN PassengerDetails PD ON PD.BookingID = B.BookingID
                    WHERE B.BookingID = @BookingID";
                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@BookingID", bookingId);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }

                // 3) Load tickets into DataTable
                string query = @"
            SELECT 
                T.TicketNumber,
                F.FlightNumber,
                SA.Name AS [From],
                DA.Name AS [To],
                CONVERT(VARCHAR, F.DepartureTime, 100) AS Departure,
                CONVERT(VARCHAR, F.ArrivalTime, 100) AS Arrival,
                PD.SeatNumber
            FROM TicketDetails T
            INNER JOIN Flights F ON T.FlightID = F.FlightID
            INNER JOIN Airports SA ON T.SourceAirportID = SA.AirportID
            INNER JOIN Airports DA ON T.DestinationAirportID = DA.AirportID
            INNER JOIN PassengerDetails PD ON PD.BookingID = T.BookingID
            WHERE T.BookingID = @BookingID";

                using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@BookingID", bookingId);
                    da.Fill(dt);
                }
            }

            gvTicket.DataSource = dt;
            gvTicket.DataBind();
            ViewState["TicketData"] = dt;
        }

        // ===================================
        // DOWNLOAD & EMAIL PDF
        // ===================================
        protected void btnDownload_Click(object sender, EventArgs e)
        {
            try
            {
                // ✅ 1. Generate PDF
                byte[] pdfBytes = GenerateTicketPDF();

                // ✅ 2. Get email address (entered or registered)
                string email = !string.IsNullOrEmpty(txtEmail.Text)
                    ? txtEmail.Text
                    : GetUserEmail(Session["UserID"].ToString());

                // ✅ 3. Send email FIRST (before Response.End)
                if (!string.IsNullOrEmpty(email))
                {
                    SendTicketEmail(email, pdfBytes);
                    lblMessage.Text = "✅ Ticket emailed to " + email;
                }
                else
                {
                    lblMessage.Text = "⚠️ No email address found.";
                }

                // ✅ 4. Then download the PDF to browser
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=GoFly_Ticket.pdf");
                Response.BinaryWrite(pdfBytes);
                Response.Flush();
                Response.SuppressContent = false;
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Error sending ticket: " + ex.Message;
            }
        }


        private byte[] GenerateTicketPDF()
        {
            DataTable tickets = ViewState["TicketData"] as DataTable;
            DataTable passengers = ViewState["PassengerList"] as DataTable;

            using (MemoryStream ms = new MemoryStream())
            {
                Document pdfDoc = new Document(PageSize.A4, 40, 40, 40, 40);
                PdfWriter.GetInstance(pdfDoc, ms);
                pdfDoc.Open();

                Paragraph title = new Paragraph("GoFly E-Ticket\n\n", FontFactory.GetFont("Arial", 20, Font.BOLD, BaseColor.BLUE));
                title.Alignment = Element.ALIGN_CENTER;
                pdfDoc.Add(title);

                pdfDoc.Add(new Paragraph("Booking ID: " + lblBookingId.Text));
                pdfDoc.Add(new Paragraph("Payment ID: " + lblPaymentId.Text));
                pdfDoc.Add(new Paragraph("Status: " + lblStatus.Text));
                pdfDoc.Add(new Paragraph("Date: " + lblDate.Text));
                pdfDoc.Add(new Paragraph("Amount: " + lblAmount.Text));
                pdfDoc.Add(new Paragraph("From: " + lblFrom.Text));
                pdfDoc.Add(new Paragraph("To: " + lblTo.Text));
                pdfDoc.Add(new Paragraph("Flight No: " + lblFlightNumber.Text));
                pdfDoc.Add(new Paragraph("Departure: " + lblDeparture.Text));
                pdfDoc.Add(new Paragraph("Arrival: " + lblArrival.Text));
                pdfDoc.Add(new Paragraph("Total Passengers: " + lblPassengers.Text + "\n\n"));

                // Passenger details
                if (passengers != null && passengers.Rows.Count > 0)
                {
                    PdfPTable ptable = new PdfPTable(passengers.Columns.Count);
                    foreach (DataColumn col in passengers.Columns)
                    {
                        PdfPCell header = new PdfPCell(new Phrase(col.ColumnName, FontFactory.GetFont("Arial", 12, Font.BOLD)));
                        header.BackgroundColor = new BaseColor(220, 220, 220);
                        ptable.AddCell(header);
                    }
                    foreach (DataRow row in passengers.Rows)
                        foreach (var c in row.ItemArray)
                            ptable.AddCell(c.ToString());
                    pdfDoc.Add(new Paragraph("Passenger Details:\n"));
                    pdfDoc.Add(ptable);
                }

                // Ticket details
                PdfPTable ttable = new PdfPTable(tickets.Columns.Count);
                foreach (DataColumn col in tickets.Columns)
                {
                    PdfPCell header = new PdfPCell(new Phrase(col.ColumnName, FontFactory.GetFont("Arial", 12, Font.BOLD)));
                    header.BackgroundColor = new BaseColor(220, 220, 220);
                    ttable.AddCell(header);
                }
                foreach (DataRow row in tickets.Rows)
                    foreach (var c in row.ItemArray)
                        ttable.AddCell(c.ToString());
                pdfDoc.Add(new Paragraph("\nTicket & Seat Details:\n"));
                pdfDoc.Add(ttable);

                pdfDoc.Add(new Paragraph("\nThank you for flying with GoFly!", FontFactory.GetFont("Arial", 12, Font.ITALIC, BaseColor.GRAY)));
                pdfDoc.Close();
                return ms.ToArray();
            }
        }

        private string GetUserEmail(string userId)
        {
            string connStr = ConfigurationManager.ConnectionStrings[ConnStrName].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT Email FROM Users_1 WHERE UserID=@UID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UID", userId);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : null;
            }
        }

        private void SendTicketEmail(string toEmail, byte[] pdfBytes)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("patelkanostudent@gmail.com", "GoFly Airlines");
            mail.To.Add(toEmail);
            mail.Subject = "Your GoFly E-Ticket Confirmation";
            mail.Body = $"Dear Passenger,<br/><br/>Please find attached your e-ticket for flight <b>{lblFlightNumber.Text}</b> from <b>{lblFrom.Text}</b> to <b>{lblTo.Text}</b>.<br/><br/>" +
                        "Thank you for choosing GoFly!<br/>Safe travels! ✈️<br/><br/>— Team GoFly";
            mail.IsBodyHtml = true;

            mail.Attachments.Add(new Attachment(new MemoryStream(pdfBytes), "GoFly_Ticket.pdf"));

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential("patelkanostudent@gmail.com", "xrvx welj nagp bsbz")
            };
            smtp.Send(mail);
        }

        protected void btnViewBookings_Click(object sender, EventArgs e)
        {
            Response.Redirect("MyBookings.aspx");
        }
    }
}
