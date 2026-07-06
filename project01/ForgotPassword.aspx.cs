using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Web.UI;

namespace project01
{
    public partial class WebForm5 : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔒 STEP 1: Security check before anything else
            if (Session["Username"] == null || Session["role"] == null)
            {
                // Session expired or user not logged in
                Response.Redirect("Login.aspx?msg=PleaseLoginFirst");
                return;
            }
            if (!IsPostBack)
            {
                pnlEmail.Visible = true;
                pnlOTP.Visible = false;
                pnlResetPassword.Visible = false;
            }
        }

        protected void btnSendOTP_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(email))
            {
                lblMessage.Text = "Enter your email.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (!IsEmailRegistered(email))
            {
                lblMessage.Text = "This email is not registered.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            // Generate 6-digit OTP
            Random rand = new Random();
            string otp = rand.Next(100000, 999999).ToString();

            // Store in session
            Session["ResetEmail"] = email;
            Session["OTP"] = otp;

            try
            {
                // Send OTP email
                SendEmailOTP(email, otp);
                lblMessage.ForeColor = System.Drawing.Color.Green;
                lblMessage.Text = $"OTP has been sent to {email}. Please check your inbox.";

                pnlEmail.Visible = false;
                pnlOTP.Visible = true;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Failed to send email: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnVerifyOTP_Click(object sender, EventArgs e)
        {
            if (Session["OTP"] == null || Session["ResetEmail"] == null)
            {
                lblMessage.Text = "Session expired. Please start again.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                pnlEmail.Visible = true;
                pnlOTP.Visible = false;
                pnlResetPassword.Visible = false;
                return;
            }

            string enteredOtp = txtOTP.Text.Trim();
            string actualOtp = Session["OTP"].ToString();

            if (enteredOtp == actualOtp)
            {
                lblMessage.Text = "OTP verified successfully.";
                lblMessage.ForeColor = System.Drawing.Color.Green;

                pnlOTP.Visible = false;
                pnlResetPassword.Visible = true;
                Session["OTPVerified"] = true;
            }
            else
            {
                lblMessage.Text = "Invalid OTP. Try again.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnResetPassword_Click(object sender, EventArgs e)
        {
            if (Session["OTPVerified"] == null || Session["ResetEmail"] == null)
            {
                lblMessage.Text = "OTP not verified.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string newPwd = txtNewPassword.Text.Trim();
            string confirmPwd = txtConfirmPassword.Text.Trim();

            if (newPwd != confirmPwd)
            {
                lblMessage.Text = "Passwords do not match.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (newPwd.Length < 7 || newPwd.Length > 16)
            {
                lblMessage.Text = "Password must be 7–16 characters.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string email = Session["ResetEmail"].ToString();
            string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            try
            {
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    SqlCommand cmd = new SqlCommand("UPDATE Users_1 SET Password=@Password WHERE Email=@Email", con);
                    cmd.Parameters.AddWithValue("@Password", newPwd);
                    cmd.Parameters.AddWithValue("@Email", email);
                    con.Open();

                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                    {
                        lblMessage.Text = "Password reset successful! Redirecting to login...";
                        lblMessage.ForeColor = System.Drawing.Color.Green;
                        Session.Clear();
                        Response.AddHeader("REFRESH", "3;URL=Login.aspx");
                    }
                    else
                    {
                        lblMessage.Text = "Error updating password.";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        private bool IsEmailRegistered(string email)
        {
            string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            string query = "SELECT COUNT(*) FROM Users_1 WHERE Email=@Email";

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                con.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        // ✅ Sends OTP Email using Gmail SMTP
        private void SendEmailOTP(string toEmail, string otp)
        {
            string fromEmail = "patelkanostudent@gmail.com";       // replace with your Gmail
            string fromPassword = "xrvx welj nagp bsbz";        // use Gmail App Password (not your normal password)

            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(fromEmail, "Password Recovery");
            msg.To.Add(toEmail);
            msg.Subject = "Your OTP Code";
            msg.Body = $"Your OTP for password reset is: {otp}\n\nDo not share this with anyone.";
            msg.IsBodyHtml = false;

            using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Credentials = new NetworkCredential(fromEmail, fromPassword);
                smtp.EnableSsl = true;
                smtp.Send(msg);
            }
        }
    }
}
