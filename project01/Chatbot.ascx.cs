using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace project01
{
    public partial class Chatbot : System.Web.UI.UserControl
    {
        protected List<Message> ChatHistory
        {
            get
            {
                if (Session["ChatHistory"] == null)
                    Session["ChatHistory"] = new List<Message>();
                return (List<Message>)Session["ChatHistory"];
            }
        }

        string cs = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
           
            if (!IsPostBack)
            {
                RenderChatHistory();

                string userDisplay = Session["username"] != null && !string.IsNullOrEmpty(Session["username"].ToString())
                    ? Session["username"].ToString()
                    : "Guest";

                ChatHistory.Add(new Message
                {
                    Text = $"👋 Hello {userDisplay}! Welcome to <b>GoFly</b> flight booking support. My name is <b>Aero</b>. How can I assist you today?",
                    IsUser = false
                });

                RenderChatHistory();
            }
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || string.IsNullOrEmpty(Session["username"] as string))
            {
                ChatHistory.Add(new Message
                {
                    Text = "⚠️ Please <a href='Login.aspx'>login</a> first to continue chatting.",
                    IsUser = false
                });

                txtChatInput.Text = "";
                RenderChatHistory();
                return;
            }

            string userMsg = txtChatInput.Text.Trim();
            if (string.IsNullOrEmpty(userMsg)) return;

            ChatHistory.Add(new Message { Text = userMsg, IsUser = true });

            string botReply = GetBotReply(userMsg);

            ChatHistory.Add(new Message { Text = botReply, IsUser = false });

            txtChatInput.Text = "";
            RenderChatHistory();
        }

        // ==========================
        // BOT REPLY HANDLER
        // ==========================
        private string GetBotReply(string userMsg)
        {
            userMsg = userMsg.ToLower();

            string userDisplay = Session["username"] != null && !string.IsNullOrEmpty(Session["username"].ToString())
                ? Session["username"].ToString()
                : "Guest";

            if (string.IsNullOrWhiteSpace(userMsg) || userMsg == "start")
                return $"Hello {userDisplay}! Welcome to GoFly flight booking support. How can I assist you today?";

            if (userMsg.Contains("hello") || userMsg.Contains("hi") || userMsg.Contains("welcome") || userMsg.Contains("gofly"))
                return $"Hello {userDisplay}! 👋 How can I help you today?";

            if (userMsg.Contains("weather"))
                return $"☀️ Hello {userDisplay}, the weather today is 36°C and clear skies.";

            if (userMsg.Contains("baggage") || userMsg.Contains("luggage"))
                return "🧳 You can carry one cabin bag and one check-in bag (max 23kg). Extra weight will incur additional charges.";

            if (userMsg.Contains("change flight") || userMsg.Contains("reschedule") || userMsg.Contains("modify booking"))
                return "🔁 You can reschedule up to 24 hours before departure, subject to fare conditions and change fees.";

            if (userMsg.Contains("cancel flight") || userMsg.Contains("cancel booking") || userMsg.Contains("cancellation"))
                return "❌ Cancellations are allowed up to 24 hours before departure. Refunds follow the fare rules.";

            if (userMsg.Contains("refund"))
                return "💸 Refunds are processed within 7–10 business days after cancellation confirmation.";

            if (userMsg.Contains("check-in") || userMsg.Contains("boarding"))
                return "🛫 Online check-in opens 24 hours before departure and closes 2 hours before the flight.";

            if (userMsg.Contains("flight status") || userMsg.Contains("flight time") || userMsg.Contains("departure time"))
                return "✈️ Please provide your flight number or ticket number to check flight status.";

            // Ticket pattern detection (like TKT571281068)
            string ticketNumber = ExtractTicketNumber(userMsg);
            if (!string.IsNullOrEmpty(ticketNumber))
                return GetTicketStatusFromDB(ticketNumber);

            return $"🤖 Hello {userDisplay}, I didn’t understand that. You can ask about your ticket (e.g., 'TKT12345') or flight information.";
        }

        // ==========================
        // EXTRACT TICKET NUMBER
        // ==========================
        private string ExtractTicketNumber(string msg)
        {
            Match match = Regex.Match(msg, @"TKT\d{5,12}", RegexOptions.IgnoreCase);
            return match.Success ? match.Value.ToUpper() : null;
        }

        // ==========================
        // FETCH TICKET DETAILS
        // ==========================
        private string GetTicketStatusFromDB(string ticketNumber)
        {
            string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT 
                        T.TicketNumber,
                        T.PassengerName,
                        T.Status AS TicketStatus,
                        T.FinalPrice,
                        F.FlightNumber,
                        SA.City AS SourceCity,
                        DA.City AS DestinationCity,
                        CONVERT(VARCHAR, F.DepartureTime, 100) AS DepartureTime,
                        CONVERT(VARCHAR, F.ArrivalTime, 100) AS ArrivalTime,
                        PD.SeatNumber
                    FROM TicketDetails T
                    INNER JOIN Flights F ON T.FlightID = F.FlightID
                    INNER JOIN Airports SA ON F.SourceAirportID = SA.AirportID
                    INNER JOIN Airports DA ON F.DestinationAirportID = DA.AirportID
                    LEFT JOIN PassengerDetails PD 
                        ON PD.BookingID = T.BookingID 
                        AND PD.Name = T.PassengerName
                    WHERE T.TicketNumber = @TicketNumber";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TicketNumber", ticketNumber);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string tNum = reader["TicketNumber"].ToString();
                            string passenger = reader["PassengerName"].ToString();
                            string status = reader["TicketStatus"].ToString();
                            string flightNum = reader["FlightNumber"].ToString();
                            string fromCity = reader["SourceCity"].ToString();
                            string toCity = reader["DestinationCity"].ToString();
                            string dep = reader["DepartureTime"].ToString();
                            string arr = reader["ArrivalTime"].ToString();
                            string seat = reader["SeatNumber"] != DBNull.Value ? reader["SeatNumber"].ToString() : "Not Assigned";
                            string price = Convert.ToDecimal(reader["FinalPrice"]).ToString("C", CultureInfo.CurrentCulture);

                            return $"🛫 <b>Ticket Details for {tNum}</b><br/>" +
                                   $"👤 Passenger: {passenger}<br/>" +
                                   $"✈️ Flight No: {flightNum}<br/>" +
                                   $"📍 From: {fromCity}<br/>" +
                                   $"🏁 To: {toCity}<br/>" +
                                   $"🕓 Departure: {dep}<br/>" +
                                   $"🕕 Arrival: {arr}<br/>" +
                                   $"💺 Seat: {seat}<br/>" +
                                   $"💵 Fare: {price}<br/>" +
                                   $"📄 Status: {status}";
                        }
                        else
                        {
                            return $"❌ Ticket number <b>{ticketNumber}</b> not found in our records.";
                        }
                    }
                }
            }
        }

        // ==========================
        // RENDER CHAT HISTORY
        // ==========================
        private void RenderChatHistory()
        {
            pnlChat.Controls.Clear();

            foreach (var msg in ChatHistory)
            {
                Label lbl = new Label();
                lbl.Text = msg.Text;
                lbl.CssClass = msg.IsUser ? "msg user" : "msg bot";
                pnlChat.Controls.Add(lbl);
                pnlChat.Controls.Add(new LiteralControl("<br/>"));
            }

            ScriptManager.RegisterStartupScript(this, GetType(), "scrollChat", "scrollChatToBottom();", true);
        }

        // ==========================
        // MESSAGE CLASS
        // ==========================
        public class Message
        {
            public string Text { get; set; }
            public bool IsUser { get; set; }
        }
    }
}
