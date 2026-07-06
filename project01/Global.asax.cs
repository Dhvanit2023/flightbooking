using System;
using System.Data.SqlClient;
using System.Web;

namespace project01
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // Initialize live user count
            Application["LiveUsers"] = 0;
        }
        protected void Application_Close(object sender, EventArgs e)
        {
            Application["LiveUsers"] = (int)Application["LiveUsers"] - 1;

        }

        protected void Session_Start(object sender, EventArgs e)
        {
            // Increase live user count
            Application.Lock();
            Application["LiveUsers"] = (int)Application["LiveUsers"] + 1;
            Application.UnLock();

            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            try
            {
                string sessionId = Session.SessionID;
                string ipAddress = HttpContext.Current.Request.UserHostAddress;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = @"
                        INSERT INTO VisitorStats (SessionID, IPAddress, VisitTime, LastActiveTime, IsActive)
                        VALUES (@SessionID, @IPAddress, GETDATE(), GETDATE(), 1)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SessionID", sessionId);
                        cmd.Parameters.AddWithValue("@IPAddress", ipAddress);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Session_Start Error: " + ex.Message);
            }
        }

        protected void Session_End(object sender, EventArgs e)
        {
            // Reduce live user count
            Application.Lock();
            Application["LiveUsers"] = (int)Application["LiveUsers"] - 1;
            Application.UnLock();

            UpdateUserStatus(false);
        }

        private void UpdateUserStatus(bool isActive)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            try
            {
                string sessionId = Session.SessionID;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = @"
                        UPDATE VisitorStats
                        SET IsActive = @IsActive, LastActiveTime = GETDATE()
                        WHERE SessionID = @SessionID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
                        cmd.Parameters.AddWithValue("@SessionID", sessionId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateUserStatus Error: " + ex.Message);
            }
        }
    }
}
