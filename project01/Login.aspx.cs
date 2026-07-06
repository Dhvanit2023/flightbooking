using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace project01
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                GenerateCaptcha();
        }

        // ---------------- CAPTCHA (IMAGE) ------------------
        private void GenerateCaptcha()
        {
            Random rnd = new Random();
            int num1 = rnd.Next(1, 20);
            int num2 = rnd.Next(1, 20);

            int result = num1 + num2;

            ViewState["CaptchaResult"] = result;

            Session["CaptchaText"] = $"{num1} + {num2}";

            imgCaptcha.ImageUrl = "CaptchaImage.ashx?t=" + DateTime.Now.Ticks;
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            GenerateCaptcha();
        }

        // ---------------- PASSWORD HASHING ------------------
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));

                return sb.ToString();
            }
        }

        // ---------------- LOGIN BUTTON ------------------
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // Validate Captcha
            if (!int.TryParse(txtCaptcha.Text.Trim(), out int enteredCaptcha) ||
                enteredCaptcha != (int)ViewState["CaptchaResult"])
            {
                lblMessage.Text = "❌ CAPTCHA is incorrect!";
                GenerateCaptcha();
                return;
            }

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            string hashedPassword = HashPassword(password);

            string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = "SELECT * FROM Users_1 WHERE Username=@Username AND Password=@Password";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", hashedPassword);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {
                    dr.Read();

                    int userID = Convert.ToInt32(dr["UserID"]);
                    string email = dr["Email"].ToString();
                    string phone = dr["Phone"].ToString();
                    string name = dr["Name"].ToString();
                    string role = Convert.ToBoolean(dr["IsAdmin"]) ? "admin" : "user";

                    // Sessions
                    Session["UserID"] = userID;
                    Session["Username"] = username;
                    Session["Email"] = email;
                    Session["Phone"] = phone;
                    Session["role"] = role;

                    string profileImage = dr["ProfileImage"] != DBNull.Value ? dr["ProfileImage"].ToString() : "";

                    Session["UserPhoto"] = string.IsNullOrEmpty(profileImage)
                        ? "Photos/defaultprofile.jpg"
                        : "Photos/Userprofile/" + profileImage;

                    dr.Close();

                    // Log login
                    string ip = Request.UserHostAddress;
                    string logQuery = "INSERT INTO UserLogs (UserID, IPAddress, LoginTime, Activity) VALUES (@U,@IP,GETDATE(),'User logged in')";

                    using (SqlCommand logCmd = new SqlCommand(logQuery, con))
                    {
                        logCmd.Parameters.AddWithValue("@U", userID);
                        logCmd.Parameters.AddWithValue("@IP", ip);
                        logCmd.ExecuteNonQuery();
                    }

                    // Verification (Email/SMS)
                    bool ok = VerificationService.SendAllVerifications(name, email, phone, Session, out string error);

                    if (ok)
                        Response.Redirect("Verify.aspx");
                    else
                        lblMessage.Text = "Verification failed: " + error;
                }
                else
                {
                    lblMessage.Text = "❌ Invalid username or password!";
                    GenerateCaptcha();
                }
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Response.Redirect("Register.aspx");
        }
    }
}
