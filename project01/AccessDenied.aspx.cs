using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace project01
{
    public partial class AccessDenied : System.Web.UI.Page
    {
       
            protected void Page_Load(object sender, EventArgs e)
            {
                // Optional: You can display username if session exists
                if (Session["Username"] != null)
                {
                    string username = Session["Username"].ToString();
                    // You could log this unauthorized access attempt if needed
                    // Example: LogUnauthorizedAccess(username);
                }
            }

            protected void btnGoHome_Click(object sender, EventArgs e)
            {
                Response.Redirect("Default.aspx");
            }

            protected void btnLogin_Click(object sender, EventArgs e)
            {
                Response.Redirect("Login.aspx");
            }
        }
    }
