using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace project01
{
    public partial class WebForm13 : System.Web.UI.Page
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
        }
    }
}