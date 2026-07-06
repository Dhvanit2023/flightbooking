using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;

namespace project01
{
    public partial class WebForm2 : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 🔒 STEP 1: Security check before anything else
                if (Session["Username"] == null || Session["role"] == null)
                {
                    // Session expired or user not logged in
                    Response.Redirect("Login.aspx?msg=PleaseLoginFirst");
                    return;
                }
                if (Session["UserID"] != null)
                {
                    int userID = Convert.ToInt32(Session["UserID"]);
                    hfUserID.Value = userID.ToString();
                    LoadProfile(userID);
                    BindUserSessions(userID);
                }
                else
                {
                    Response.Redirect("Login.aspx");
                }
            }
        }

        private void LoadProfile(int userID)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = "SELECT * FROM Users_1 WHERE UserID=@UserID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserID", userID);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    txtName.Text = dr["Name"].ToString();
                    txtEmail.Text = dr["Email"].ToString();
                    txtUsername.Text = dr["Username"].ToString();
                    txtPhone.Text = dr["Phone"].ToString();
                    ViewState["Password"] = dr["Password"].ToString();

                    string profileImage = dr["ProfileImage"]?.ToString();
                    imgProfile.ImageUrl = !string.IsNullOrEmpty(profileImage) ? "~/" + profileImage : "~/Photos/defaultprofile.jpg";
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string relativePath = null;
            if (fuProfileImage.HasFile)
            {
                string ext = Path.GetExtension(fuProfileImage.FileName).ToLower();
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                {
                    string fileName = Guid.NewGuid() + ext;
                    relativePath = "Photos/Userprofile/" + fileName;
                    fuProfileImage.SaveAs(Server.MapPath("~/" + relativePath));
                }
                else
                {
                    lblError.Text = "Only JPG, JPEG, PNG allowed!";
                    return;
                }
            }

            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = "UPDATE Users_1 SET Name=@Name, Username=@Username, Phone=@Phone" +
                               (relativePath != null ? ", ProfileImage=@ProfileImage" : "") +
                               " WHERE UserID=@UserID";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Name", txtName.Text);
                cmd.Parameters.AddWithValue("@Username", txtUsername.Text);
                cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                if (relativePath != null)
                    cmd.Parameters.AddWithValue("@ProfileImage", relativePath);
                cmd.Parameters.AddWithValue("@UserID", Convert.ToInt32(hfUserID.Value));

                con.Open();
                cmd.ExecuteNonQuery();
            }

            lblMessage.Text = "Profile updated successfully!";
            LoadProfile(Convert.ToInt32(hfUserID.Value));
        }

        private void BindUserSessions(int userID)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = "SELECT LogID, IPAddress, LoginTime, LogoutTime, Activity FROM UserLogs WHERE UserID=@UserID ORDER BY LoginTime DESC";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserID", userID);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvUserSessions.DataSource = dt;
                gvUserSessions.DataBind();
            }
        }

        protected void btnTerminateAll_Click(object sender, EventArgs e)
        {
            if (Session["UserID"] != null)
            {
                int userID = Convert.ToInt32(Session["UserID"]);
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string query = "UPDATE UserLogs SET LogoutTime=GETDATE(), Activity='User logged out' WHERE UserID=@UserID AND LogoutTime IS NULL";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                Session.Abandon();
                Session.Clear();
                Response.Redirect("Login.aspx");
            }
        }
    }
}
