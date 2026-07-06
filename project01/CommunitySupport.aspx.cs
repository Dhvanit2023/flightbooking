using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;

namespace project01
{
    public partial class CommunitySupport : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

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
                LoadChat();
        }

        private void LoadChat()
        {
            StringBuilder sb = new StringBuilder();
            string currentUser = Session["Username"].ToString();

            using (SqlConnection con = new SqlConnection(connStr))
            {
                // Select messages and order by ChatID (or Timestamp) to show chronologically
                string query = "SELECT Username, MessageText, SenderType, Timestamp FROM SupportChat ORDER BY ChatID ASC";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    string sender = dr["Username"].ToString();
                    string msg = dr["MessageText"].ToString();
                    string senderType = dr["SenderType"].ToString();
                    string time = Convert.ToDateTime(dr["Timestamp"]).ToString("dd MMM hh:mm tt");

                    // Determine CSS class based on who sent the message
                    bool isCurrentUser = sender.Equals(currentUser, StringComparison.OrdinalIgnoreCase);
                    string css = (senderType == "Admin" || !isCurrentUser) ? "msg msg-admin" : "msg msg-user";

                    // Determine display name
                    string displayName = senderType == "Admin" ? "🧑‍💼 Admin" :
                        isCurrentUser ? "You" : sender;

                    // Reverse the floating logic for the user's message to be on the right
                    if (isCurrentUser && senderType != "Admin")
                    {
                        // User message (floated right)
                        sb.Append($@"
                            <div style='clear:both; text-align:right;'>
                                <div class='{css}'>
                                    <div class='username'>{displayName}</div>
                                    <div>{HttpUtility.HtmlEncode(msg)}</div>
                                    <div class='timestamp'>{time}</div>
                                </div>
                            </div>");
                    }
                    else
                    {
                        // Admin or Other User message (floated left)
                        sb.Append($@"
                            <div style='clear:both; text-align:left;'>
                                <div class='{css}'>
                                    <div class='username'>{displayName}</div>
                                    <div>{HttpUtility.HtmlEncode(msg)}</div>
                                    <div class='timestamp'>{time}</div>
                                </div>
                            </div>");
                    }
                }
                dr.Close();
                con.Close();
            }

            chatBox.InnerHtml = sb.ToString();

            // Re-run the scroll script after the chat content is loaded/updated
            ScriptManager.RegisterStartupScript(this, GetType(), "ScrollChat", "ScrollChatToBottom();", true);
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            string msg = txtMessage.Text.Trim();
            if (msg == "")
                return;

            string username = Session["Username"].ToString();
            // Assuming "Admin" username is used for Admin users.
            string senderType = username.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User";

            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = "INSERT INTO SupportChat (Username, MessageText, SenderType, Timestamp) VALUES (@Username, @MessageText, @SenderType, GETDATE())";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@MessageText", msg);
                cmd.Parameters.AddWithValue("@SenderType", senderType);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            txtMessage.Text = "";
            LoadChat();
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            LoadChat();
        }
    }
}