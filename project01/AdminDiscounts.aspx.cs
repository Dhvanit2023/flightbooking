using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace project01
{
    public partial class WebForm23 : System.Web.UI.Page
    {
        private readonly string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

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
                LoadDiscounts();
        }

        private void LoadDiscounts()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = "SELECT * FROM Discounts ORDER BY DiscountID DESC";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvDiscounts.DataSource = dt;
                gvDiscounts.DataBind();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                SqlCommand cmd;
                if (string.IsNullOrEmpty(hfDiscountID.Value))
                {
                    cmd = new SqlCommand(@"INSERT INTO Discounts (Code, Description, Percentage, ValidTill)
                                           VALUES (@Code, @Desc, @Per, @ValidTill)", con);
                    lblMessage.Text = "✅ Coupon added successfully!";
                }
                else
                {
                    cmd = new SqlCommand(@"UPDATE Discounts 
                                           SET Code=@Code, Description=@Desc, Percentage=@Per, ValidTill=@ValidTill
                                           WHERE DiscountID=@ID", con);
                    cmd.Parameters.AddWithValue("@ID", hfDiscountID.Value);
                    lblMessage.Text = "✏ Coupon updated successfully!";
                }

                cmd.Parameters.AddWithValue("@Code", txtCode.Text.Trim());
                cmd.Parameters.AddWithValue("@Desc", txtDescription.Text.Trim());
                cmd.Parameters.AddWithValue("@Per", Convert.ToDecimal(txtPercentage.Text.Trim()));
                cmd.Parameters.AddWithValue("@ValidTill", txtValidTill.Text);
                cmd.ExecuteNonQuery();
            }

            ClearForm();
            LoadDiscounts();
        }

        protected void gvDiscounts_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvDiscounts.EditIndex = e.NewEditIndex;
            LoadDiscounts();
        }

        protected void gvDiscounts_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvDiscounts.EditIndex = -1;
            LoadDiscounts();
        }

        protected void gvDiscounts_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = gvDiscounts.Rows[e.RowIndex];
            int id = Convert.ToInt32(gvDiscounts.DataKeys[e.RowIndex].Value);

            string code = ((TextBox)row.FindControl("txtEditCode")).Text;
            string desc = ((TextBox)row.FindControl("txtEditDescription")).Text;
            string per = ((TextBox)row.FindControl("txtEditPercentage")).Text;
            string valid = ((TextBox)row.FindControl("txtEditValidTill")).Text;

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(@"UPDATE Discounts 
                                                  SET Code=@Code, Description=@Desc, Percentage=@Per, ValidTill=@ValidTill 
                                                  WHERE DiscountID=@ID", con);
                cmd.Parameters.AddWithValue("@Code", code);
                cmd.Parameters.AddWithValue("@Desc", desc);
                cmd.Parameters.AddWithValue("@Per", Convert.ToDecimal(per));
                cmd.Parameters.AddWithValue("@ValidTill", valid);
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }

            gvDiscounts.EditIndex = -1;
            LoadDiscounts();
            lblMessage.Text = "💾 Coupon updated successfully!";
        }

        protected void gvDiscounts_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = Convert.ToInt32(gvDiscounts.DataKeys[e.RowIndex].Value);
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM Discounts WHERE DiscountID=@ID", con);
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }
            LoadDiscounts();
            lblMessage.Text = "🗑 Coupon deleted successfully!";
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hfDiscountID.Value = "";
            txtCode.Text = "";
            txtDescription.Text = "";
            txtPercentage.Text = "";
            txtValidTill.Text = "";
        }
    }
}
