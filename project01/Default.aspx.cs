using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace project01
{
    public partial class WebForm3 : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDashboardStats();
                LoadAverageRating();
                LoadHotDeals();
                LoadTravelNews();
                }
        }

        // ✅ Dynamic flight statistics
        private void LoadDashboardStats()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();

                // Total Bookings Confirmed
                SqlCommand cmd1 = new SqlCommand("SELECT COUNT(*) FROM Bookings WHERE Status='Confirmed'", con);
                int totalFlights = Convert.ToInt32(cmd1.ExecuteScalar());

                // Total Passengers
                SqlCommand cmd2 = new SqlCommand("SELECT ISNULL(SUM(SeatsBooked),0) FROM Bookings WHERE Status='Confirmed'", con);
                int totalPassengers = Convert.ToInt32(cmd2.ExecuteScalar());

                // Families count (group of 4)
                int families = totalPassengers / 4;

                // Happy customers
                SqlCommand cmd3 = new SqlCommand("SELECT COUNT(*) FROM Feedbacks WHERE Rating >= 4", con);
                int happyCustomers = Convert.ToInt32(cmd3.ExecuteScalar());

                // ---------------- VisitorStats Queries ----------------

                // Total Visitors
                SqlCommand v1 = new SqlCommand("SELECT COUNT(*) FROM VisitorStats", con);
                int totalVisitors = Convert.ToInt32(v1.ExecuteScalar());

                // Today Visitors
                SqlCommand v2 = new SqlCommand("SELECT COUNT(*) FROM VisitorStats WHERE CAST(VisitTime AS DATE)=CAST(GETDATE() AS DATE)", con);
                int todayVisitors = Convert.ToInt32(v2.ExecuteScalar());

                // Live Active Users
                SqlCommand v3 = new SqlCommand("SELECT COUNT(*) FROM VisitorStats WHERE IsActive = 1", con);
                int liveUsers = Convert.ToInt32(v3.ExecuteScalar());

                // Unique Visitors (distinct IP)
                SqlCommand v4 = new SqlCommand("SELECT COUNT(DISTINCT IPAddress) FROM VisitorStats", con);
                int uniqueVisitors = Convert.ToInt32(v4.ExecuteScalar());


                // ---------------- Set Labels ----------------
                lblTotalFlights.Text = totalFlights.ToString();
                lblTotalPassengers.Text = totalPassengers.ToString();
                lblFamilies.Text = families.ToString();
                lblHappyCustomers.Text = happyCustomers.ToString();

                Label1.Text = totalVisitors.ToString();
               // lblTodayVisitors.Text = todayVisitors.ToString();
                //lblLiveUsers.Text = liveUsers.ToString();
                //lblUniqueVisitors.Text = uniqueVisitors.ToString();
            }

        }

        // ✅ Load Hot Flight Deals from DB
        private void LoadHotDeals()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT TOP 6 * FROM HotDeals ORDER BY DealID DESC", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptHotDeals.DataSource = dt;
                rptHotDeals.DataBind();
            }
        }

        // ✅ Load Travel News from DB
        private void LoadTravelNews()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT TOP 6 * FROM TravelNews ORDER BY NewsID DESC", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptTravelNews.DataSource = dt;
                rptTravelNews.DataBind();
            }
        }

        protected void btnSubmitFeedback_Click(object sender, EventArgs e)
        {
            int rating = int.Parse(hfRating.Value);
            string feedbackText = txtFeedback.Text.Trim();

            if (rating == 0)
            {
                lblFeedbackMsg.Text = "⚠️ Please select a rating before submitting.";
                lblFeedbackMsg.CssClass = "text-danger";
                return;
            }

            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = "INSERT INTO Feedbacks (Rating, FeedbackText, CreatedAt) VALUES (@Rating, @FeedbackText, GETDATE())";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Rating", rating);
                cmd.Parameters.AddWithValue("@FeedbackText", feedbackText);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            lblFeedbackMsg.Text = "✅ Thank you for your feedback!";
            lblFeedbackMsg.CssClass = "text-success";
            txtFeedback.Text = "";
            hfRating.Value = "0";
            LoadAverageRating();
            LoadDashboardStats();
        }

        private void LoadAverageRating()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = "SELECT AVG(CAST(Rating AS FLOAT)) AS AvgRating, COUNT(*) AS Total FROM Feedbacks";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    double avg = dr["AvgRating"] != DBNull.Value ? Math.Round(Convert.ToDouble(dr["AvgRating"]), 1) : 0;
                    int total = dr["Total"] != DBNull.Value ? Convert.ToInt32(dr["Total"]) : 0;

                    lblAverageRating.Text = $"⭐ Average Rating: {avg}/5";
                    lblTotalReviews.Text = $"({total} reviews)";
                }
            }
        }


      
 
    }
}
