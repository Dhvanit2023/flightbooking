using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace project01
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        // 🔐 Function to hash password using SHA256
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);

                // Convert hash to readable string
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            string phone = txtPhone.Text.Trim();

            // hash the password before storing 🔐
            string hashPass = HashPassword(password);

            string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = @"INSERT INTO Users_1 
                                (Name, Email, Username, Password, Phone) 
                                VALUES (@Name, @Email, @Username, @Password, @Phone)";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", hashPass); // store hashed password
                    cmd.Parameters.AddWithValue("@Phone", phone);

                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();

                        lblStatus.ForeColor = System.Drawing.Color.Green;
                        lblStatus.Text = "Registration successful!";

                        Response.Redirect("Login.aspx");
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2627)
                        {
                            lblStatus.Text = "Username, Email, or Phone already exists!";
                        }
                        else
                        {
                            lblStatus.Text = "Error: " + ex.Message;
                        }
                    }
                }
            }
        }
    }
}
