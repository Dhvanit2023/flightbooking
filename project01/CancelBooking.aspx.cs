using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Web.UI;

namespace project01
{
    public partial class WebForm16 : System.Web.UI.Page
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
        }

        // ✅ Step 1: Send OTP (after Booking + Payment verification)
        protected void btnSendOtp_Click(object sender, EventArgs e)
        {
            lblMessage.Text = lblError.Text = "";
            try
            {
                if (string.IsNullOrEmpty(txtBookingID.Text) || string.IsNullOrEmpty(txtPaymentID.Text))
                {
                    lblError.Text = "⚠️ Please enter both Booking ID and Payment ID.";
                    return;
                }

                int bookingId = Convert.ToInt32(txtBookingID.Text);
                string paymentId = txtPaymentID.Text.Trim();
                string userId = Session[SessionKeyUserId].ToString();

                if (!VerifyBookingAndPayment(bookingId, paymentId, userId))
                {
                    lblError.Text = "❌ Invalid Booking ID or Payment ID. Please check your details.";
                    return;
                }

                string email = GetUserEmail(userId);
                if (string.IsNullOrEmpty(email))
                {
                    lblError.Text = "⚠️ No registered email found.";
                    return;
                }

                // Generate OTP
                Random rnd = new Random();
                int otp = rnd.Next(100000, 999999);

                Session["CancelOtp"] = otp;
                Session["CancelBookingID"] = bookingId;
                Session["CancelPaymentID"] = paymentId;

                SendOtpEmail(email, otp);

                lblMessage.Text = "✅ OTP sent successfully to your registered email.";
                pnlVerify.Visible = true;
            }
            catch (Exception ex)
            {
                lblError.Text = "❌ Error sending OTP: " + ex.Message;
            }
        }

        // ✅ Step 2: Verify OTP and Cancel Booking
        protected void btnVerifyOtp_Click(object sender, EventArgs e)
        {
            lblMessage.Text = lblError.Text = "";
            try
            {
                if (Session["CancelOtp"] == null || Session["CancelBookingID"] == null)
                {
                    lblError.Text = "⚠️ OTP expired or not generated. Please try again.";
                    return;
                }

                int enteredOtp = Convert.ToInt32(txtOtp.Text);
                int actualOtp = Convert.ToInt32(Session["CancelOtp"]);
                int bookingId = Convert.ToInt32(Session["CancelBookingID"]);

                if (enteredOtp != actualOtp)
                {
                    lblError.Text = "❌ Incorrect OTP. Please try again.";
                    return;
                }

                string connStr = ConfigurationManager.ConnectionStrings[ConnStrName].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();

                    try
                    {
                        // Fetch booking info
                        int flightClassId = 0, seatsBooked = 0;
                        string q1 = "SELECT FlightClassID, SeatsBooked FROM Bookings WHERE BookingID=@BookingID AND UserID=@UserID";
                        using (SqlCommand cmd = new SqlCommand(q1, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@BookingID", bookingId);
                            cmd.Parameters.AddWithValue("@UserID", Session[SessionKeyUserId]);
                            SqlDataReader r = cmd.ExecuteReader();
                            if (r.Read())
                            {
                                flightClassId = Convert.ToInt32(r["FlightClassID"]);
                                seatsBooked = Convert.ToInt32(r["SeatsBooked"]);
                            }
                            else
                            {
                                r.Close();
                                throw new Exception("Booking not found or unauthorized.");
                            }
                            r.Close();
                        }

                        // Cancel Booking
                        string q2 = "UPDATE Bookings SET Status='Cancelled', PaymentDate=NULL WHERE BookingID=@BookingID";
                        new SqlCommand(q2, conn, tran)
                        {
                            Parameters = { new SqlParameter("@BookingID", bookingId) }
                        }.ExecuteNonQuery();

                        // Cancel Tickets
                        new SqlCommand("UPDATE TicketDetails SET Status='Cancelled' WHERE BookingID=@BID", conn, tran)
                        {
                            Parameters = { new SqlParameter("@BID", bookingId) }
                        }.ExecuteNonQuery();

                        // Refund Payment
                        new SqlCommand("UPDATE Payments SET PaymentStatus='Refund Initiated' WHERE BookingID=@BID", conn, tran)
                        {
                            Parameters = { new SqlParameter("@BID", bookingId) }
                        }.ExecuteNonQuery();

                        // Restore seats
                        new SqlCommand("UPDATE FlightClasses SET SeatsAvailable = SeatsAvailable + @S WHERE FlightClassID=@FC",
                            conn, tran)
                        {
                            Parameters =
                            {
                                new SqlParameter("@S", seatsBooked),
                                new SqlParameter("@FC", flightClassId)
                            }
                        }.ExecuteNonQuery();

                        // Log cancellation
                        new SqlCommand(@"INSERT INTO BookingCancellationLog (BookingID, UserID, CancelledAt, Reason,RefundAmount)
                                         VALUES (@B, @U, GETDATE(), 'Cancelled by user via OTP & Payment verification',@amount)",
                            conn, tran)
                        {
                            Parameters =
                            {
                                new SqlParameter("@B", bookingId),
                                new SqlParameter("@U", Session[SessionKeyUserId]),
                                new SqlParameter("@amount",3000)
                            }
                        }.ExecuteNonQuery();
                        // Cancel Tickets
                        new SqlCommand("UPDATE TicketDetails SET Status='Cancelled' WHERE BookingID=@BID", conn, tran)
                        {
                            Parameters = { new SqlParameter("@BID", bookingId) }
                        }.ExecuteNonQuery();

                        // Cancel Seats (Free them)
                        new SqlCommand(@"UPDATE BookedSeats 
                 SET IsCancelled = 1, CancelDate = GETDATE() 
                 WHERE BookingID = @BID", conn, tran)
                        {
                            Parameters = { new SqlParameter("@BID", bookingId) }
                        }.ExecuteNonQuery();

                        tran.Commit();
                        lblMessage.Text = "✅ Booking cancelled successfully. Refund process initiated.";

                        string email = GetUserEmail(Session[SessionKeyUserId].ToString());
                        if (!string.IsNullOrEmpty(email))
                            SendCancelConfirmation(email, bookingId);

                        Session.Remove("CancelOtp");
                        Session.Remove("CancelBookingID");
                        Session.Remove("CancelPaymentID");
                    }
                    catch (Exception ex2)
                    {
                        tran.Rollback();
                        throw new Exception("Transaction failed: " + ex2.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "❌ Error: " + ex.Message;
            }
        }

        // ✅ Verify that Booking ID and Payment ID belong to same user
        private bool VerifyBookingAndPayment(int bookingId, string paymentId, string userId)
        {
            string connStr = ConfigurationManager.ConnectionStrings[ConnStrName].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT COUNT(*) FROM Payments P
                                 INNER JOIN Bookings B ON P.BookingID = B.BookingID
                                 WHERE P.RazorpayPaymentID = @PID AND B.BookingID = @BID AND B.UserID = @UID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PID", paymentId);
                cmd.Parameters.AddWithValue("@BID", bookingId);
                cmd.Parameters.AddWithValue("@UID", userId);
                conn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
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

        private void SendOtpEmail(string toEmail, int otp)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("patelkanostudent@gmail.com", "GoFly Airlines");
            mail.To.Add(toEmail);
            mail.Subject = "GoFly Booking Cancellation OTP";
            mail.Body = $"Dear Customer,<br/><br/>Your OTP for cancelling your booking is: <b>{otp}</b>.<br/><br/>" +
                        "Please use this within 10 minutes.<br/>— Team GoFly";
            mail.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential("patelkanostudent@gmail.com", "xrvx welj nagp bsbz") // your app password
            };
            smtp.Send(mail);
        }

        private void SendCancelConfirmation(string toEmail, int bookingId)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("patelkanostudent@gmail.com", "GoFly Airlines");
            mail.To.Add(toEmail);
            mail.Subject = "Your GoFly Booking Has Been Cancelled";
            mail.Body = $"Dear Customer,<br/><br/>Your booking <b>#{bookingId}</b> has been successfully cancelled.<br/>" +
                        "Your refund has been initiated and will reflect within 7–10 business days.<br/><br/>" +
                        "Thank you for flying with GoFly.<br/>— Team GoFly";
            mail.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential("patelkanostudent@gmail.com", "xrvx welj nagp bsbz")
            };
            smtp.Send(mail);
        }
    }
}
