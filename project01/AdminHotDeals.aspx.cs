using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace project01
{
    public partial class WebForm24 : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Username"] == null || Session["role"] == null)
            {
                Response.Redirect("Login.aspx?msg=PleaseLoginFirst");
                return;
            }

            if (!Session["role"].ToString().Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("AccessDenied.aspx");
                return;
            }

            if (!IsPostBack)
                LoadDeals();
        }

        private void LoadDeals()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM HotDeals ORDER BY DealID DESC", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvDeals.DataSource = dt;
                gvDeals.DataBind();
            }
        }

        // ---------------------- SAVE NEW DEAL ----------------------
        protected void btnSave_Click(object sender, EventArgs e)
        {
            string fullDates = txtFromDate.Text + " - " + txtToDate.Text;

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO HotDeals (Route,Price,TravelDates,ImageUrl) VALUES (@r,@p,@d,@i)", con);

                cmd.Parameters.AddWithValue("@r", txtRoute.Text);
                cmd.Parameters.AddWithValue("@p", txtPrice.Text);
                cmd.Parameters.AddWithValue("@d", fullDates);
                cmd.Parameters.AddWithValue("@i", txtImage.Text);

                cmd.ExecuteNonQuery();
            }

            lblMsg.Text = "✅ Hot deal added!";
            ClearForm();
            LoadDeals();
        }

        // ---------------------- GRIDVIEW EDIT ----------------------

        protected void gvDeals_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvDeals.EditIndex = e.NewEditIndex;
            LoadDeals();
        }

        protected void gvDeals_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvDeals.EditIndex = -1;
            LoadDeals();
        }

        // ---------------------- UPDATE DEAL ----------------------

        protected void gvDeals_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = gvDeals.Rows[e.RowIndex];

            int id = Convert.ToInt32(gvDeals.DataKeys[e.RowIndex].Value);
            string route = ((TextBox)row.FindControl("txtEditRoute")).Text;
            string price = ((TextBox)row.FindControl("txtEditPrice")).Text;
            string dates = ((TextBox)row.FindControl("txtEditDates")).Text;
            string image = ((TextBox)row.FindControl("txtEditImage")).Text;

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE HotDeals SET Route=@r,Price=@p,TravelDates=@d,ImageUrl=@i WHERE DealID=@id", con);

                cmd.Parameters.AddWithValue("@r", route);
                cmd.Parameters.AddWithValue("@p", price);
                cmd.Parameters.AddWithValue("@d", dates);
                cmd.Parameters.AddWithValue("@i", image);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
            }

            gvDeals.EditIndex = -1;
            LoadDeals();
            lblMsg.Text = "✏️ Deal updated!";
        }

        // ---------------------- DELETE DEAL ----------------------

        protected void gvDeals_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = Convert.ToInt32(gvDeals.DataKeys[e.RowIndex].Value);

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM HotDeals WHERE DealID=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            LoadDeals();
            lblMsg.Text = "🗑️ Deal deleted!";
        }

        // ---------------------- CLEAR FORM ----------------------

        protected void btnClear_Click(object sender, EventArgs e) => ClearForm();

        private void ClearForm()
        {
            txtRoute.Text = "";
            txtPrice.Text = "";
            txtFromDate.Text = "";
            txtToDate.Text = "";
            txtImage.Text = "";
        }

        // ---------------------- CALENDAR FROM DATE ----------------------

        protected void btnShowFrom_Click(object sender, EventArgs e)
        {
            calFrom.Visible = !calFrom.Visible;
        }

        protected void calFrom_SelectionChanged(object sender, EventArgs e)
        {
            txtFromDate.Text = calFrom.SelectedDate.ToString("dd MMM yyyy");
            calFrom.Visible = false;
        }

        // ---------------------- CALENDAR TO DATE ----------------------

        protected void btnShowTo_Click(object sender, EventArgs e)
        {
            calTo.Visible = !calTo.Visible;
        }

        protected void calTo_SelectionChanged(object sender, EventArgs e)
        {
            txtToDate.Text = calTo.SelectedDate.ToString("dd MMM yyyy");
            calTo.Visible = false;
        }
    }
}
