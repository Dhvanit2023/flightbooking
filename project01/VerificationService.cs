using System;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.SessionState;
using Twilio;
using Twilio.Jwt.AccessToken;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace project01
{
    public static class VerificationService
    {
        // Email config
        private const string EmailFrom = "example@gmail.com";
        private const string EmailPassword = "from google tocken";
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;

        // Twilio config (Commented for now)

        //private const string TwilioSid = "twiliosid";
        private const string TwilioSid = "twiliosid";
        private const string TwilioAuthToken = "TwilioAuthToken";
        private const string TwilioPhoneNumber = "+12294587338";


        // Main method to send OTP & link
        public static bool SendAllVerifications(string name, string email, string phone, HttpSessionState session, out string error)
        {
            string otp = GenerateOtp();
            string token = GenerateToken();

            session["OTP"] = otp;
            session["EmailToken"] = token;
            session["IsVerified"] = false;

            // ✅ Send only email now
            bool emailSent = SendEmailWithOtpAndLink(email, name, otp, token, out string emailErr);

            // ❌ Commented SMS part
            bool smsSent = SendOtpSms(phone, otp, token, out string smsErr);

            if (emailSent)
            {
                error = null;
                return true;
            }
            else
            {
                error = $"Email: {emailErr ?? "OK"}, SMS: skipped";
                return false;
            }
        }

        public static string GenerateOtp()
        {
            Random rnd = new Random();
            return rnd.Next(100000, 999999).ToString();
        }

        public static string GenerateToken()
        {
            return Guid.NewGuid().ToString();
        }

        // ✅ This sends both OTP and link in same email
        public static bool SendEmailWithOtpAndLink(string toEmail, string name, string otp, string token, out string errorMessage)
        {
            try
            {
                string verifyUrl = $"http://localhost:50450/ConfirmEmail.aspx?token={token}";

                var message = new MailMessage(EmailFrom, toEmail)
                {
                    Subject = "Your OTP is ",
                    Body = $"Hi {name},\n\n" +
                           $"Your OTP is: {otp}\n\n" +
                           $"Click this link to verify now:\n{verifyUrl}\n\n" +
                           $"Thank you."
                };

                var smtp = new SmtpClient(SmtpServer, SmtpPort)
                {
                    Credentials = new NetworkCredential(EmailFrom, EmailPassword),
                    EnableSsl = true
                };

                smtp.Send(message);
                errorMessage = null;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Email send failed: " + ex.Message;
                return false;
            }
        }

        // ❌ SMS sending completely commented

        public static bool SendOtpSms(string toPhone, string otp, string token, out string errorMessage)
        {
            try
            {
                if (!toPhone.StartsWith("+"))
                {
                    errorMessage = "Phone number must be in international format (e.g., +91xxxxxxxxxx)";
                    return false;
                }

                TwilioClient.Init(TwilioSid, TwilioAuthToken);
                string verifyUrl = $"http://localhost:50450/ConfirmEmail.aspx?token={token}";

                var message = MessageResource.Create(
                    body: $"Your OTP is: {otp} OR " +
                           $"Click this link to verify now:\n{verifyUrl}\n\n" +
                           $"Thank you.",
                    from: new PhoneNumber(TwilioPhoneNumber),
                    to: new PhoneNumber(toPhone)
                );

                errorMessage = null;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "SMS send failed: " + ex.Message;
                return false;
            }
        }


        public static bool IsOtpValid(string enteredOtp, HttpSessionState session)
        {
            return session["OTP"] != null && enteredOtp == session["OTP"].ToString();
        }

        public static bool IsEmailTokenValid(string token, HttpSessionState session)
        {
            return session["EmailToken"] != null && token == session["EmailToken"].ToString();
        }

        internal static bool SendEmailOTP(string email, out string error)
        {
            throw new NotImplementedException();
        }

        internal static bool SendEmailOTP(string email, out string error, HttpSessionState session)
        {
            throw new NotImplementedException();
        }
    }

}