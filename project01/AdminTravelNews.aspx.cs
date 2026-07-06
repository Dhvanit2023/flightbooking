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
    public partial class WebForm25 : System.Web.UI.Page
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
                    LoadNews();
            }

            private void LoadNews()
            {
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM TravelNews ORDER BY NewsID DESC", con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvNews.DataSource = dt;
                    gvNews.DataBind();
                }
            }

            protected void btnSave_Click(object sender, EventArgs e)
            {
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO TravelNews (Title, Description, Link) VALUES (@t,@d,@l)", con);
                    cmd.Parameters.AddWithValue("@t", txtTitle.Text);
                    cmd.Parameters.AddWithValue("@d", txtDescription.Text);
                    cmd.Parameters.AddWithValue("@l", txtLink.Text);
                    cmd.ExecuteNonQuery();
                }
                lblMsg.Text = "✅ News added successfully!";
                ClearForm();
                LoadNews();
            }

            protected void gvNews_RowEditing(object sender, System.Web.UI.WebControls.GridViewEditEventArgs e)
            {
                gvNews.EditIndex = e.NewEditIndex;
                LoadNews();
            }

            protected void gvNews_RowCancelingEdit(object sender, System.Web.UI.WebControls.GridViewCancelEditEventArgs e)
            {
                gvNews.EditIndex = -1;
                LoadNews();
            }

            protected void gvNews_RowUpdating(object sender, System.Web.UI.WebControls.GridViewUpdateEventArgs e)
            {
                GridViewRow row = gvNews.Rows[e.RowIndex];
                int id = Convert.ToInt32(gvNews.DataKeys[e.RowIndex].Value);
                string title = ((TextBox)row.FindControl("txtEditTitle")).Text;
                string desc = ((TextBox)row.FindControl("txtEditDescription")).Text;
                string link = ((TextBox)row.FindControl("txtEditLink")).Text;

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE TravelNews SET Title=@t, Description=@d, Link=@l WHERE NewsID=@id", con);
                    cmd.Parameters.AddWithValue("@t", title);
                    cmd.Parameters.AddWithValue("@d", desc);
                    cmd.Parameters.AddWithValue("@l", link);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                gvNews.EditIndex = -1;
                LoadNews();
                lblMsg.Text = "✏️ News updated!";
            }

            protected void gvNews_RowDeleting(object sender, System.Web.UI.WebControls.GridViewDeleteEventArgs e)
            {
                int id = Convert.ToInt32(gvNews.DataKeys[e.RowIndex].Value);
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM TravelNews WHERE NewsID=@id", con);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                LoadNews();
                lblMsg.Text = "🗑️ News deleted!";
            }

            protected void btnClear_Click(object sender, EventArgs e) => ClearForm();

            private void ClearForm()
            {
                txtTitle.Text = txtDescription.Text = txtLink.Text = "";
            }
        }
    }
