using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Globalization;

namespace project01
{
    public partial class WebForm9 : System.Web.UI.Page
    {
        private const string ConnStrName = "ConnectionString";
        private const string SessionKeyUserId = "UserID";
        private const string SessionKeyBookingId = "CurrentBookingId";
        private const string SessionKeyTotalAmount = "PaymentTotalAmount";
        private const string SessionKeyDiscountPercent = "PaymentDiscountPercent";

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
                LoadPaymentData();
            }
        }

        private void LoadPaymentData()
        {
            if (Request.QueryString["amount"] == null)
            {
                Response.Redirect("Flights.aspx");
                return;
            }

            if (!decimal.TryParse(Request.QueryString["amount"], out decimal totalAmount))
            {
                Response.Redirect("Flights.aspx");
                return;
            }

            if (Session[SessionKeyBookingId] == null)
            {
                lblDiscountMessage.Text = "Booking session lost. Please restart the booking.";
                lblDiscountMessage.ForeColor = System.Drawing.Color.Red;
                btnPay.Enabled = false;
                return;
            }

            Session[SessionKeyTotalAmount] = totalAmount;
            Session[SessionKeyDiscountPercent] = 0m;

            RecalculateAmounts(totalAmount, 0m);
        }

        protected void btnApplyDiscount_Click(object sender, EventArgs e)
        {
            if (Session[SessionKeyTotalAmount] == null)
            {
                lblDiscountMessage.Text = "Session expired. Please restart booking.";
                lblDiscountMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string discountCode = txtDiscount.Text.Trim();
            decimal totalAmount = (decimal)Session[SessionKeyTotalAmount];
            decimal discountPercent = 0m;

            if (string.IsNullOrEmpty(discountCode))
            {
                lblDiscountMessage.Text = "No code entered. Discount reset.";
                RecalculateAmounts(totalAmount, 0m);
                return;
            }

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings[ConnStrName].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = "SELECT Percentage FROM Discounts WHERE Code=@Code AND ValidTill>=GETDATE()";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Code", discountCode);

                    conn.Open();
                    object result = cmd.ExecuteScalar();

                    if (result != null && decimal.TryParse(result.ToString(), out discountPercent))
                    {
                        lblDiscountMessage.Text = $"Discount '{discountCode}' applied! ({discountPercent}% off)";
                        lblDiscountMessage.ForeColor = System.Drawing.Color.Green;
                        Session[SessionKeyDiscountPercent] = discountPercent;
                    }
                    else
                    {
                        lblDiscountMessage.Text = "Invalid or expired code.";
                        lblDiscountMessage.ForeColor = System.Drawing.Color.Red;
                        Session[SessionKeyDiscountPercent] = 0m;
                    }
                }
            }
            catch (Exception ex)
            {
                lblDiscountMessage.Text = "Error checking discount: " + ex.Message;
                lblDiscountMessage.ForeColor = System.Drawing.Color.Red;
            }

            RecalculateAmounts(totalAmount, (decimal)Session[SessionKeyDiscountPercent]);
        }

        private void RecalculateAmounts(decimal totalAmount, decimal discountPercent)
        {
            decimal discountAmt = totalAmount * (discountPercent / 100m);
            decimal finalAmount = totalAmount - discountAmt;
            if (finalAmount < 0) finalAmount = 0;

            lblTotalAmount.Text = totalAmount.ToString("C", CultureInfo.CurrentCulture);
            lblDiscountAmount.Text = discountAmt.ToString("C", CultureInfo.CurrentCulture);
            lblFinalAmount.Text = finalAmount.ToString("C", CultureInfo.CurrentCulture);
            hdnFinalAmount.Value = finalAmount.ToString("F2", CultureInfo.InvariantCulture);
        }

        protected void btnPay_Click(object sender, EventArgs e)
        {
            // Not used (Razorpay handled client-side)
        }
    }
}
