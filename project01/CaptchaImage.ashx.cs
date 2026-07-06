
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.SessionState;

namespace project01
{
    public class CaptchaImage : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.Clear();
            context.Response.Buffer = true;
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();

            // Get captcha text
            string captchaText = context.Session["CaptchaText"] as string;

            // If null (during fast refresh), generate a fallback
            if (string.IsNullOrEmpty(captchaText))
            {
                Random r = new Random();
                int a = r.Next(1, 5);
                int b = r.Next(1, 5);

                captchaText = $"{a} + {b}";
                context.Session["CaptchaText"] = captchaText;
            }

            using (Bitmap bmp = new Bitmap(150, 60))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);

                using (Font font = new Font("Arial", 26, FontStyle.Bold))
                {
                    g.DrawString(captchaText, font, Brushes.DarkBlue, 10, 10);
                }

                Pen pen = new Pen(Color.Gray, 1);
                Random rnd = new Random();

                // Noise lines
                for (int i = 0; i < 10; i++)
                {
                    g.DrawLine(pen,
                        rnd.Next(0, 150), rnd.Next(0, 60),
                        rnd.Next(0, 150), rnd.Next(0, 60));
                }

                // Save into memory buffer (fixes ArgumentException)
                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Png);

                    context.Response.ContentType = "image/png";
                    context.Response.BinaryWrite(ms.ToArray());
                }
            }
        }

        public bool IsReusable => false;
    }
}
