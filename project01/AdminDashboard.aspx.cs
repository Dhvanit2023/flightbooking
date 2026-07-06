using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace project01
{
    // Ensure this class name matches the Inherits attribute in the .aspx file
    public partial class WebForm17 : System.Web.UI.Page
    {
        // Connection string access
        private readonly string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        // Note: These GridView/Button controls must be declared either here or in WebForm17.designer.cs
        // for the C# code to compile correctly.
        // Example:
        // protected System.Web.UI.WebControls.GridView gvHotSectors;
        // protected System.Web.UI.WebControls.Button btnExportPdf; 


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDropdowns();
                LoadSummaryCounts();
                ApplyDefaultPeriod();
                BindRecentBookings();
            }
            // Ensure date textboxes are disabled unless 'custom' is selected on every postback
            txtFrom.Enabled = ddlPeriod.SelectedValue == "custom";
            txtTo.Enabled = ddlPeriod.SelectedValue == "custom";
        }

        #region Load UI helpers
        private void LoadDropdowns()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();

                // 1. Flight classes
                using (SqlCommand cmd = new SqlCommand("SELECT FlightClassID, ClassName FROM FlightClasses ORDER BY ClassName", con))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    ddlClass.Items.Clear();
                    ddlClass.Items.Add(new System.Web.UI.WebControls.ListItem("All Classes", ""));
                    while (r.Read())
                    {
                        ddlClass.Items.Add(new System.Web.UI.WebControls.ListItem(r["ClassName"].ToString(), r["FlightClassID"].ToString()));
                    }
                }

                // 2. Airports for source/destination
                using (SqlCommand cmd2 = new SqlCommand("SELECT AirportID, Name + ' (' + City + ')' AS Display FROM Airports ORDER BY City, Name", con))
                using (SqlDataReader r2 = cmd2.ExecuteReader())
                {
                    ddlSource.Items.Clear();
                    ddlDestination.Items.Clear();
                    ddlSource.Items.Add(new System.Web.UI.WebControls.ListItem("All Sources", ""));
                    ddlDestination.Items.Add(new System.Web.UI.WebControls.ListItem("All Destinations", ""));
                    while (r2.Read())
                    {
                        var li = new System.Web.UI.WebControls.ListItem(r2["Display"].ToString(), r2["AirportID"].ToString());
                        ddlSource.Items.Add(li);
                        ddlDestination.Items.Add(new System.Web.UI.WebControls.ListItem(r2["Display"].ToString(), r2["AirportID"].ToString()));
                    }
                }
            }
        }

       
        private void LoadSummaryCounts()
        {
            decimal refund = 0;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                

                using (SqlConnection con1 = new SqlConnection(connStr))
                {
                    con.Open();

                    string sql1 = @"SELECT ISNULL(SUM(RefundAmount), 0) AS RefundAmount 
                    FROM BookingCancellationLog";

                    using (SqlCommand cmd = new SqlCommand(sql1, con))
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            refund = Convert.ToDecimal(r["RefundAmount"]);
                        }
                    }
                }


                // Get all-time totals for the dashboard cards
                string sql = @"
                    SELECT 
                        (SELECT COUNT(1) FROM Flights) AS TotalFlights,
                        (SELECT COUNT(1) FROM Users_1) AS TotalUsers,
                        (SELECT COUNT(1) FROM Bookings) AS TotalBookings,
                        (SELECT COUNT(1) FROM Bookings WHERE Status = 'Cancelled') AS CancelledTickets,
                        (SELECT ISNULL(SUM(Amount),0) FROM Payments WHERE PaymentStatus = 'Completed') AS TotalRevenue
                ";
                using (SqlCommand cmd = new SqlCommand(sql, con))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        lblTotalFlights.Text = r["TotalFlights"].ToString();
                        lblTotalUsers.Text = r["TotalUsers"].ToString();
                        lblTotalBookings.Text = r["TotalBookings"].ToString();
                        lblCancelledTickets.Text = r["CancelledTickets"].ToString();
                        
                        //lblNetProfitLoss2.Text = Convert.ToDecimal(lblTotalRevenue) - refund;
                        decimal totalRevenue = Convert.ToDecimal(r["TotalRevenue"]);
                        lblTotalRevenue.Text = totalRevenue.ToString();
                        decimal netRevenue = totalRevenue - refund;

                        lblTotalRevenue.Text = totalRevenue.ToString("N2");
                        lblNetProfitLoss2.Text = netRevenue.ToString("N2");
                        decimal Totalrefund = totalRevenue - netRevenue ;
                        Label1.Text = Totalrefund.ToString();

                    }
                }
            }
        }

        private void ApplyDefaultPeriod()
        {
            ddlPeriod.SelectedValue = "week";
            GetDateRangeFromUI(out DateTime from, out DateTime to);
            txtFrom.Text = from.ToString("yyyy-MM-dd");
            txtTo.Text = to.Date.ToString("yyyy-MM-dd");
            txtFrom.Enabled = false;
            txtTo.Enabled = false;
        }
        #endregion

        #region Binding bookings & reports

        protected void btnApplyFilter_Click(object sender, EventArgs e)
        {
            BindRecentBookings();
        }

        private void BindRecentBookings()
        {
            DateTime from, to;
            GetDateRangeFromUI(out from, out to);

            string username = txtUsername.Text.Trim();
            string classId = ddlClass.SelectedValue;
            string sourceId = ddlSource.SelectedValue;
            string destId = ddlDestination.SelectedValue;

            // 1. Get filtered booking data
            DataTable dt = GetBookingsReport(from, to, username, classId, sourceId, destId);
            gvRecentBookings.DataSource = dt;
            gvRecentBookings.DataBind();

            // 2. Update Profit/Loss Summary based on filtered data
            UpdateProfitLoss(dt);

            // 3. Calculate and display Hot Sectors
            DisplayHotSectors(dt);
        }

        private void UpdateProfitLoss(DataTable dt)
        {
            decimal revenue = 0;
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow r in dt.Rows)
                {
                    if (r["Status"].ToString() == "Confirmed")
                    {
                        if (decimal.TryParse(r["FinalPrice"].ToString(), out decimal price))
                        {
                            // Use price for calculation
                            revenue += price;   // example
                        }
                    }

                }
            }

            lblRevenueDetails.Text = revenue.ToString("N2");
            decimal operationalCost = Math.Round(revenue * 0.35M, 2);
            lblOperationalCost.Text = operationalCost.ToString("N2");
            decimal net = revenue - operationalCost;
            lblNetProfitLoss.Text = net.ToString(net >= 0 ? "N2" : "N2", CultureInfo.InvariantCulture);

            // Set color based on profit/loss
            lblNetProfitLoss.Style["color"] = net < 0 ? "#dc3545" : "#28a745";

            lblProfitMargin.Text = revenue > 0 ? ((net / revenue) * 100).ToString("N2") + "%" : "0%";
            lblProfitMargin.Style["color"] = net < 0 ? "#dc3545" : "#28a745";

            divProfitLoss.Style["display"] = "block";
        }

        private void DisplayHotSectors(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                gvHotSectors.DataSource = null;
                gvHotSectors.DataBind();
                return;
            }

            // Group by SourceCity and DestinationCity, count, and take top 5
            var hotSectors = dt.AsEnumerable()
                .GroupBy(r => new { From = r.Field<string>("SourceCity"), To = r.Field<string>("DestinationCity") })
                .Select(g => new
                {
                    Route = $"{g.Key.From} -> {g.Key.To}",
                    BookingsCount = g.Count()
                })
                .OrderByDescending(x => x.BookingsCount)
                .Take(5)
                .ToList();

            gvHotSectors.DataSource = hotSectors;
            gvHotSectors.DataBind();
        }

        private void GetDateRangeFromUI(out DateTime from, out DateTime to)
        {
            string period = ddlPeriod.SelectedValue ?? "week";
            DateTime now = DateTime.Now;

            switch (period)
            {
                case "day":
                    from = now.Date;
                    to = now.Date.AddDays(1).AddTicks(-1);
                    break;
                case "week":
                    from = now.Date.AddDays(-7);
                    to = now.Date.AddDays(1).AddTicks(-1);
                    break;
                case "month":
                    from = new DateTime(now.Year, now.Month, 1);
                    to = from.AddMonths(1).AddTicks(-1);
                    break;
                case "year":
                    from = new DateTime(now.Year, 1, 1);
                    to = from.AddYears(1).AddTicks(-1);
                    break;
                case "custom":
                    if (!DateTime.TryParseExact(txtFrom.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out from))
                        from = now.Date.AddDays(-7);
                    if (!DateTime.TryParseExact(txtTo.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime toDateOnly))
                        to = now.Date.AddDays(1).AddTicks(-1);
                    else
                        to = toDateOnly.Date.AddDays(1).AddTicks(-1);
                    break;
                default:
                    from = now.Date.AddDays(-7);
                    to = now.Date.AddDays(1).AddTicks(-1);
                    break;
            }

            if (from > to)
            {
                from = to.Date.AddDays(-7);
            }
        }

        private DataTable GetBookingsReport(DateTime from, DateTime to, string username, string classId, string sourceId, string destId)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"
                    SELECT b.BookingID, u.Username, u.Name, f.FlightNumber, fc.ClassName, b.SeatsBooked, b.FinalPrice,
                           b.Status, b.BookingDate, sa.City AS SourceCity, da.City AS DestinationCity
                    FROM Bookings b
                    INNER JOIN Users_1 u ON b.UserID = u.UserID
                    INNER JOIN Flights f ON b.FlightID = f.FlightID
                    INNER JOIN FlightClasses fc ON b.FlightClassID = fc.FlightClassID
                    INNER JOIN Airports sa ON f.SourceAirportID = sa.AirportID
                    INNER JOIN Airports da ON f.DestinationAirportID = da.AirportID
                    WHERE b.BookingDate >= @FromDate AND b.BookingDate <= @ToDate");

                using (SqlCommand cmd = new SqlCommand(sb.ToString(), con))
                {
                    cmd.Parameters.AddWithValue("@FromDate", from);
                    cmd.Parameters.AddWithValue("@ToDate", to);

                    if (!string.IsNullOrEmpty(username))
                    {
                        cmd.CommandText += " AND (u.Username LIKE @Username OR u.Name LIKE @Username)";
                        cmd.Parameters.AddWithValue("@Username", "%" + username + "%");
                    }
                    if (!string.IsNullOrEmpty(classId))
                    {
                        cmd.CommandText += " AND b.FlightClassID = @ClassID";
                        cmd.Parameters.AddWithValue("@ClassID", Convert.ToInt32(classId));
                    }
                    if (!string.IsNullOrEmpty(sourceId))
                    {
                        cmd.CommandText += " AND f.SourceAirportID = @SourceID";
                        cmd.Parameters.AddWithValue("@SourceID", Convert.ToInt32(sourceId));
                    }
                    if (!string.IsNullOrEmpty(destId))
                    {
                        cmd.CommandText += " AND f.DestinationAirportID = @DestID";
                        cmd.Parameters.AddWithValue("@DestID", Convert.ToInt32(destId));
                    }

                    cmd.CommandText += " ORDER BY b.BookingDate DESC";

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        #endregion

        #region Period dropdown changed
        protected void ddlPeriod_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetDateRangeFromUI(out DateTime f, out DateTime t);
            txtFrom.Text = f.ToString("yyyy-MM-dd");
            txtTo.Text = t.Date.ToString("yyyy-MM-dd");

            if (ddlPeriod.SelectedValue == "custom")
            {
                txtFrom.Enabled = true;
                txtTo.Enabled = true;
            }
            else
            {
                txtFrom.Enabled = false;
                txtTo.Enabled = false;
                BindRecentBookings();
            }
        }
        #endregion

        #region Export Handlers (CSV and PDF)

        protected void btnExportCsv_Click(object sender, EventArgs e)
        {
            DateTime from, to;
            GetDateRangeFromUI(out from, out to);
            var dt = GetBookingsReport(from, to, txtUsername.Text.Trim(), ddlClass.SelectedValue, ddlSource.SelectedValue, ddlDestination.SelectedValue);
            ExportDataTableToCsv(dt, "bookings_report.csv");
        }

        private void ExportDataTableToCsv(DataTable dt, string filename)
        {
            if (dt == null || dt.Rows.Count == 0) return;
            StringBuilder sb = new StringBuilder();

            // Use the BoundField HeaderText for user-friendly CSV headers
            string[] headers = gvRecentBookings.Columns.Cast<DataControlField>().Select(c => c.HeaderText).ToArray();
            sb.AppendLine(string.Join(",", headers.Select(h => "\"" + h.Replace("\"", "\"\"") + "\"")));

            // Rows
            foreach (DataRow row in dt.Rows)
            {
                string[] fields = new string[dt.Columns.Count];
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    fields[i] = row[i].ToString().Replace("\"", "\"\"").Replace("\r", "").Replace("\n", "");
                }
                sb.AppendLine(string.Join(",", fields.Select(f => "\"" + f + "\"")));
            }

            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.ContentType = "text/csv";
            System.Web.HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + filename);
            System.Web.HttpContext.Current.Response.Write(sb.ToString());
            System.Web.HttpContext.Current.Response.End();
        }

        protected void btnExportPdf_Click(object sender, EventArgs e)
        {
            DateTime from, to;
            GetDateRangeFromUI(out from, out to);
            var dt = GetBookingsReport(from, to, txtUsername.Text.Trim(), ddlClass.SelectedValue, ddlSource.SelectedValue, ddlDestination.SelectedValue);
            ExportDataTableToPdf(dt, "bookings_report.pdf");
        }

        private void ExportDataTableToPdf(DataTable dt, string filename)
        {
            // 1. Safety Check
            if (dt == null || dt.Rows.Count == 0)
                return;

            // 2. Create Document
            Document doc = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
            MemoryStream ms = new MemoryStream();

            try
            {
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // ================= TITLE =================
                Paragraph title = new Paragraph(
                    "Admin Booking Report",
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16f)
                );
                title.Alignment = Element.ALIGN_CENTER;
                doc.Add(title);

                Paragraph period = new Paragraph(
                     $"Period: {txtFrom.Text} to {txtTo.Text} | Records: {dt.Rows.Count}",
                     FontFactory.GetFont(FontFactory.HELVETICA, 10f)
                );
                period.Alignment = Element.ALIGN_CENTER;
                doc.Add(period);

                doc.Add(Chunk.NEWLINE);

                // ================= NEW FONT METHOD (NO FILE REQUIRED) =================
                // We use GetFont instead of CreateFont. This uses internal iTextSharp fonts.
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.WHITE);
                Font rowFont = FontFactory.GetFont(FontFactory.HELVETICA, 7, BaseColor.BLACK);

                // ================= TABLE =================
                PdfPTable table = new PdfPTable(dt.Columns.Count);
                table.WidthPercentage = 100;
                table.SpacingBefore = 10f;

                BaseColor headerBg = new BaseColor(0, 123, 255);

                // -------- HEADERS --------
                foreach (DataColumn col in dt.Columns)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(col.ColumnName, headerFont));
                    cell.BackgroundColor = headerBg;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 5;
                    table.AddCell(cell);
                }

                // Find price column index for alignment
                int priceIndex = dt.Columns.Contains("FinalPrice") ? dt.Columns["FinalPrice"].Ordinal : -1;

                // -------- ROWS --------
                foreach (DataRow row in dt.Rows)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        string text = row[i] == DBNull.Value ? "" : row[i].ToString();

                        PdfPCell cell = new PdfPCell(new Phrase(text, rowFont));
                        cell.Padding = 4;

                        // Align Price to Right, everything else Left
                        cell.HorizontalAlignment = (i == priceIndex) ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT;

                        table.AddCell(cell);
                    }
                }

                doc.Add(table);
                doc.Close();

                // ================= DOWNLOAD =================
                Response.Clear();
                Response.Buffer = true;
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "attachment;filename=" + filename);
                Response.BinaryWrite(ms.ToArray());
                Response.Flush();
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                // Simple error logging or re-throw
                throw new Exception("PDF Export Failed: " + ex.Message);
            }
            finally
            {
                ms.Dispose();
                doc.Dispose();
            }
        }
        #endregion

        #region Other Redirect Handlers
        protected void btnManageFlights_Click(object sender, EventArgs e) { Response.Redirect("AdminManageFlights.aspx"); }
        protected void btnManageUsers_Click(object sender, EventArgs e) { Response.Redirect("AdminManageUsers.aspx"); }
        protected void btnManagePayments_Click(object sender, EventArgs e) { Response.Redirect("AdminPayments.aspx"); }
        protected void btnSupport_Click(object sender, EventArgs e) { Response.Redirect("AdminSupportChat.aspx"); }
        protected void btnFlightStatus_Click(object sender, EventArgs e) { Response.Redirect("AdminManageFlightStatus.aspx"); }
        protected void btnFlightStats_Click(object sender, EventArgs e) { Response.Redirect("AdminFlightStatistics.aspx"); }
        protected void btnDiscounts_Click(object sender, EventArgs e) { Response.Redirect("AdminDiscounts.aspx"); }
        protected void btnHotDeals_Click(object sender, EventArgs e) { Response.Redirect("AdminHotDeals.aspx"); }
        protected void btnTravelNews_Click(object sender, EventArgs e) { Response.Redirect("AdminTravelNews.aspx"); }

        protected void lnkRevenue_Click(object sender, EventArgs e)
        {
            divProfitLoss.Style["display"] = (divProfitLoss.Style["display"] == "block") ? "none" : "block";
        }
        #endregion
    }
}