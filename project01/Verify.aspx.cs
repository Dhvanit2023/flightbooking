using System;

namespace project01
{
    public partial class WebForm7 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // On first load, redirect if already verified
            if (!IsPostBack)
            {
                if (Session["IsVerified"] != null && (bool)Session["IsVerified"])
                {
                    Response.Redirect("default.aspx");
                }
            }
        }

        protected void btnVerifyOTP_Click(object sender, EventArgs e)
        {
            string enteredOtp = txtOTP.Text.Trim();

            if (VerificationService.IsOtpValid(enteredOtp, Session))
            {
                Session["IsVerified"] = true;
                lblMessage.ForeColor = System.Drawing.Color.Green;
                lblMessage.Text = "OTP verified! Redirecting...";
                Response.Redirect("default.aspx");
            }
            else
            {
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Text = "Invalid OTP. Please try again.";
            }
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            // Periodic check every 3 seconds if user is verified by email link (or OTP)
            if (Session["IsVerified"] != null && (bool)Session["IsVerified"])
            {
                string redirectUrl = "default.aspx";

                // If user came from a protected page (like SetBooking.aspx), go back there
                if (Session["ReturnUrl"] != null)
                {
                    redirectUrl = Session["ReturnUrl"].ToString();
                    Session.Remove("ReturnUrl"); // clear after redirect
                }

                // Redirect immediately
                Response.Redirect(redirectUrl, false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

    }
}
