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
    public partial class WebForm28 : System.Web.UI.Page
    {
       
            string cs = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            protected void Page_Load(object sender, EventArgs e)
            {
                if (!IsPostBack)
                {
                    LoadDeals();
                }
            }

            private void LoadDeals()
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    // Select only the columns needed, ordered by newest first
                    string query = "SELECT DealID, Route, Price, TravelDates, ImageUrl, CreatedAt FROM HotDeals ORDER BY CreatedAt DESC";

                    SqlCommand cmd = new SqlCommand(query, con);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();

                    con.Open();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        rptDeals.DataSource = dt;
                        rptDeals.DataBind();
                    }
                    else
                    {
                        lblNoDeals.Visible = true; // Show "No Deals" message
                    }
                }
            }
        }
    }