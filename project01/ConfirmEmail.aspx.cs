using System;

namespace project01
{
    public partial class WebForm8 : System.Web.UI.Page
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
                string token = Request.QueryString["token"];

                if (!string.IsNullOrEmpty(token))
                {
                    if (VerificationService.IsEmailTokenValid(token, Session))
                    {
                        Session["IsVerified"] = true;
                        lblMessage.CssClass = "success";
                        lblMessage.Text = " verified successfully.";
                       
                    }
                    else
                    {
                        lblMessage.CssClass = "error";
                        lblMessage.Text = "Invalid or expired verification link.";
                    }
                }
                else
                {
                    lblMessage.CssClass = "error";
                    lblMessage.Text = "Verification token is missing.";
                }
            }
        }
    }
}
