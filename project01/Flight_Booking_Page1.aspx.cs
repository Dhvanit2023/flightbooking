using System;
using System.Net.PeerToPeer;
using System.Web;
using System.Web.UI.WebControls;

namespace project01
{
    public partial class Flight_Booking_Page1 : System.Web.UI.Page
    {
        // Connection string and other parts are omitted for brevity, as they were correct.

        protected void Page_Load(object sender, EventArgs e)
        {
            // Define a variable to hold the condition result
            bool isUserLoggedIn = (Session["UserID"] != null);

            if (Session["UserID"] == null)
            {
                Session["ReturnUrl"] = Request.RawUrl; // full URL of current page
                Response.Redirect("Login.aspx");
            }


            if (!IsPostBack)
            {
 
                // Reading parameters passed from Flights.aspx
                string flightId = Request.QueryString["flightId"];
                string flightClassId = Request.QueryString["classId"];
                string basePriceStr = Request.QueryString["basePrice"];
                string discountAmount1 = Session["lo_dis"].ToString();

                string className = Request.QueryString["className"];
                string from = Request.QueryString["from"];
                string to = Request.QueryString["to"];
                string depart = Request.QueryString["depart"];
                string adStr = Request.QueryString["ad"] ?? "1";
                string chStr = Request.QueryString["ch"] ?? "0";
                string srStr = Request.QueryString["sr"] ?? "0";

                if (string.IsNullOrEmpty(flightId) || string.IsNullOrEmpty(flightClassId) || string.IsNullOrEmpty(basePriceStr))
                {
                    Response.Write("<script>alert('Missing flight or pricing details.');window.location='Flights.aspx';</script>");
                    return;
                }

                int adults = Convert.ToInt32(adStr);
                int children = Convert.ToInt32(chStr);
                int seniors = Convert.ToInt32(srStr);
                decimal basePrice = Convert.ToDecimal(basePriceStr);
                decimal deicountp = Convert.ToDecimal(discountAmount1);
                int totalPassengers = adults + children + seniors; // 💡 NEW: Calculate total passengers

                // Calculate final price based on passenger mix
                decimal finalPrice = CalculateFinalPrice(adults, children, seniors, basePrice,deicountp);

                // Display details
                lblFlightID.Text = $"{flightId} / {flightClassId}";
                lblClass.Text = HttpUtility.UrlDecode(className ?? "");
                lblFrom.Text = HttpUtility.UrlDecode(from ?? "");
                lblTo.Text = HttpUtility.UrlDecode(to ?? "");
                lblDepartDate.Text = depart ?? "";
                lblPassengers.Text = $"{adults} Adult(s) / {children} Child(ren) / {seniors} Senior(s)";
                lblBasePrice.Text = $"₹{basePrice:N0}";
                l_discount.Text= $"₹{deicountp:N0}";
                lblFinalPrice.Text = $"₹{finalPrice:N0}";

                // Store ALL essential booking data in ViewState, including totalPassengers
                ViewState["flightId"] = flightId;
                ViewState["flightClassId"] = flightClassId;
                ViewState["className"] = className; // Added class name for passing
                ViewState["basePrice"] = basePriceStr; // Store as string for easy pass
                ViewState["totalPassengers"] = totalPassengers.ToString(); // Store total passenger count
                ViewState["finalPrice"] = finalPrice;

                // ... (ad, ch, sr can be kept if needed for later pages)
            }
        }

        private decimal CalculateFinalPrice(int adults, int children, int seniors, decimal basePrice,decimal deicountp)
        {
            decimal finalPrice = basePrice - deicountp;
            decimal effectivePassengers = adults + (children * 0.6m) + (seniors * 0.7m);
            return Math.Round(effectivePassengers * finalPrice, 2);
        }

        // 🟢 THE FIX IS HERE: Passing all required details to the seat selection page.
        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            // Retrieve data from ViewState
            string flightId = ViewState["flightId"]?.ToString();
            string flightClassId = ViewState["flightClassId"]?.ToString();
            string basePrice = ViewState["basePrice"]?.ToString();
            string className = ViewState["className"]?.ToString();
            string totalPassengers = ViewState["totalPassengers"]?.ToString();

            if (string.IsNullOrEmpty(flightId) || string.IsNullOrEmpty(flightClassId) || string.IsNullOrEmpty(basePrice) || string.IsNullOrEmpty(className) || string.IsNullOrEmpty(totalPassengers))
            {
                // Should not happen if Page_Load executed correctly, but good for safety.
                Response.Write("<script>alert('Internal error: Missing booking data.');window.location='Flights.aspx';</script>");
                return;
            }

            // Construct the Query String with ALL 5 required parameters
            string url = $"selectticket.aspx?flightId={flightId}&classId={flightClassId}&className={className}&basePrice={ViewState["finalPrice"]}&totalPassengers={totalPassengers}";

            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}