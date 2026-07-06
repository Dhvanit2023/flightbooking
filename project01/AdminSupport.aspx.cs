using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace project01
{
    public partial class WebForm26 : System.Web.UI.Page
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

            // 🔒 STEP 2: Optional — restrict to admin only
            string role = Session["role"].ToString();
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                // Not an admin — block access
                Response.Redirect("AccessDenied.aspx");
                return;
            }
            if (!IsPostBack)
                    LoadChat();
            }

            // Load chat messages
            private void LoadChat()
            {
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM SupportChat ORDER BY ChatID ASC", con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    rptChat.DataSource = dt;
                    rptChat.DataBind();
                }
            }

            protected void btnSend_Click(object sender, EventArgs e)
            {
                if (string.IsNullOrWhiteSpace(txtMessage.Text))
                {
                    lblMsg.Text = "⚠️ Please type a message.";
                    lblMsg.CssClass = "text-danger";
                    return;
                }

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    string query = "INSERT INTO SupportChat (Username, MessageText, SenderType) VALUES (@u, @m, @s)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@u", "Admin");
                    cmd.Parameters.AddWithValue("@m", txtMessage.Text.Trim());
                    cmd.Parameters.AddWithValue("@s", "Admin");

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                txtMessage.Text = "";
                lblMsg.Text = "✅ Message sent!";
                lblMsg.CssClass = "text-success";

                LoadChat();
            }
        }
    }
