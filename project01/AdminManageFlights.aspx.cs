using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace project01
{
    public partial class WebForm18 : System.Web.UI.Page
    {
        private readonly string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Security checks
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
            {
                LoadAirports();
                LoadAircrafts();
                LoadFlights();
            }
        }

        #region --- Load base data ---

        private void LoadAirports()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT AirportID, Name + ' (' + Code + ')' AS Display FROM Airports ORDER BY Name", con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Bind to Add New Flight dropdowns
                ddlSource.DataSource = dt;
                ddlSource.DataTextField = "Display";
                ddlSource.DataValueField = "AirportID";
                ddlSource.DataBind();

                ddlDestination.DataSource = dt.Copy();
                ddlDestination.DataTextField = "Display";
                ddlDestination.DataValueField = "AirportID";
                ddlDestination.DataBind();
            }
        }

        private void LoadAircrafts()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT AircraftID, Name + ' (' + Model + ')' AS Display FROM Aircrafts ORDER BY Name", con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Bind to Add New Flight dropdown
                ddlAircraft.DataSource = dt;
                ddlAircraft.DataTextField = "Display";
                ddlAircraft.DataValueField = "AircraftID";
                ddlAircraft.DataBind();
            }
        }

        private void LoadFlights()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT 
                        F.FlightID,
                        F.FlightNumber,
                        A1.Name + ' (' + A1.Code + ')' AS SourceAirport,
                        A2.Name + ' (' + A2.Code + ')' AS DestinationAirport,
                        F.DepartureTime,
                        F.ArrivalTime,
                        AC.Name AS AircraftName,
                        F.SourceAirportID,
                        F.DestinationAirportID,
                        F.AircraftID
                    FROM Flights F
                    INNER JOIN Airports A1 ON F.SourceAirportID = A1.AirportID
                    INNER JOIN Airports A2 ON F.DestinationAirportID = A2.AirportID
                    INNER JOIN Aircrafts AC ON F.AircraftID = AC.AircraftID
                    ORDER BY F.FlightID DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Store in ViewState for GridView paging without re-querying DB
                ViewState["FlightsTable"] = dt;

                gvFlights.DataSource = dt;
                gvFlights.DataBind();
            }
        }

        #endregion

        #region --- CRUD Flights (Main Grid) ---

        // UPDATED: btnSave_Click will insert Flight AND class rows from tblNewClasses
        protected void btnSave_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";

            string flightNumber = txtFlightNumber.Text.Trim();

            // Input validation
            if (string.IsNullOrEmpty(flightNumber))
            {
                lblMessage.Text = "⚠ Flight number required!";
                return;
            }

            if (ddlSource.SelectedValue == ddlDestination.SelectedValue)
            {
                lblMessage.Text = "⚠ Source and Destination airports must be different!";
                return;
            }

            if (!int.TryParse(ddlSource.SelectedValue, out int sourceId) ||
                !int.TryParse(ddlDestination.SelectedValue, out int destId))
            {
                lblMessage.Text = "⚠ Invalid airport selection!";
                return;
            }

            if (!int.TryParse(ddlAircraft.SelectedValue, out int aircraftId))
            {
                lblMessage.Text = "⚠ Select aircraft!";
                return;
            }

            if (!DateTime.TryParse(txtDeparture.Text, out DateTime departure))
            {
                lblMessage.Text = "⚠ Invalid departure time! Use browser picker.";
                return;
            }

            if (!DateTime.TryParse(txtArrival.Text, out DateTime arrival))
            {
                lblMessage.Text = "⚠ Invalid arrival time! Use browser picker.";
                return;
            }

            if (departure >= arrival)
            {
                lblMessage.Text = "⚠ Departure time must be before Arrival time!";
                return;
            }

            // Read new class rows posted from inputs (names: newClassName, newBasePrice, newMultiplier, newSeats)
            string[] classNames = Request.Form.GetValues("newClassName");
            string[] basePrices = Request.Form.GetValues("newBasePrice");
            string[] multipliers = Request.Form.GetValues("newMultiplier");
            string[] seatsArr = Request.Form.GetValues("newSeats");

            if (classNames == null || classNames.Length == 0)
            {
                lblMessage.Text = "⚠ Add at least one flight class!";
                return;
            }

            // Validate class rows
            var classRows = new System.Collections.Generic.List<(string Name, decimal BasePrice, decimal Mult, int Seats)>();
            for (int i = 0; i < classNames.Length; i++)
            {
                string nm = (classNames[i] ?? "").Trim();
                if (string.IsNullOrWhiteSpace(nm))
                {
                    lblMessage.Text = $"⚠ Class name required for row {i + 1}!";
                    return;
                }

                if (basePrices == null || basePrices.Length <= i || !decimal.TryParse(basePrices[i], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal bp))
                {
                    lblMessage.Text = $"⚠ Invalid base price for '{nm}'!";
                    return;
                }

                if (multipliers == null || multipliers.Length <= i || !decimal.TryParse(multipliers[i], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal mul))
                {
                    lblMessage.Text = $"⚠ Invalid price multiplier for '{nm}'!";
                    return;
                }

                if (seatsArr == null || seatsArr.Length <= i || !int.TryParse(seatsArr[i], out int seats))
                {
                    lblMessage.Text = $"⚠ Invalid seats value for '{nm}'!";
                    return;
                }

                classRows.Add((nm, bp, mul, seats));
            }

            int newFlightId = 0;

            // Insert Flight and FlightClasses within a transaction
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                using (SqlTransaction tx = con.BeginTransaction())
                {
                    try
                    {
                        SqlCommand cmdInsFlight = new SqlCommand(@"
                            INSERT INTO Flights
                            (FlightNumber, SourceAirportID, DestinationAirportID, DepartureTime, ArrivalTime, AircraftID)
                            VALUES (@FN, @SRC, @DST, @DEP, @ARR, @AID);
                            SELECT SCOPE_IDENTITY()", con, tx);

                        cmdInsFlight.Parameters.AddWithValue("@FN", flightNumber);
                        cmdInsFlight.Parameters.AddWithValue("@SRC", sourceId);
                        cmdInsFlight.Parameters.AddWithValue("@DST", destId);
                        cmdInsFlight.Parameters.AddWithValue("@DEP", departure);
                        cmdInsFlight.Parameters.AddWithValue("@ARR", arrival);
                        cmdInsFlight.Parameters.AddWithValue("@AID", aircraftId);

                        object obj = cmdInsFlight.ExecuteScalar();
                        newFlightId = Convert.ToInt32(obj);

                        // Insert each class
                        foreach (var r in classRows)
                        {
                            SqlCommand cmdInsClass = new SqlCommand(@"
                                INSERT INTO FlightClasses (FlightID, ClassName, BasePrice, PriceMultiplier, SeatsAvailable)
                                VALUES (@FID, @Name, @Base, @Mul, @Seats)", con, tx);

                            cmdInsClass.Parameters.AddWithValue("@FID", newFlightId);
                            cmdInsClass.Parameters.AddWithValue("@Name", r.Name);
                            cmdInsClass.Parameters.AddWithValue("@Base", r.BasePrice);
                            cmdInsClass.Parameters.AddWithValue("@Mul", r.Mult);
                            cmdInsClass.Parameters.AddWithValue("@Seats", r.Seats);

                            cmdInsClass.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { tx.Rollback(); } catch { }
                        lblMessage.Text = "❌ Error saving flight and classes: " + ex.Message;
                        return;
                    }
                }
            }

            // Success - clear and reload
            lblMessage.Text = "✅ Flight and classes added successfully!";
            ClearForm();
            LoadFlights();
        }

        private void ClearForm()
        {
            txtFlightNumber.Text = "";
            txtDeparture.Text = "";
            txtArrival.Text = "";
            // Reset DropDownLists only if they have items
            if (ddlSource.Items.Count > 0) ddlSource.SelectedIndex = 0;
            if (ddlDestination.Items.Count > 0) ddlDestination.SelectedIndex = 0;
            if (ddlAircraft.Items.Count > 0) ddlAircraft.SelectedIndex = 0;

            // Reset client-side new class rows to a single default row
            string resetJs = @"
(function(){
    var tbody = document.querySelector('#tblNewClasses tbody');
    if(tbody){
        tbody.innerHTML = '<tr>' +
            '<td><select name=""newClassName"">' +
                '<option>Economy</option>' +
                '<option>Premium Economy</option>' +
                '<option>Business</option>' +
                '<option>First Class</option>' +
            '</select></td>' +
            '<td><input name=""newBasePrice"" type=""number"" step=""0.01"" min=""0"" required /></td>' +
            '<td><input name=""newMultiplier"" type=""number"" step=""0.1"" min=""0.1"" value=""1"" required /></td>' +
            '<td><input name=""newSeats"" type=""number"" min=""1"" required /></td>' +
            '<td><button type=""button"" class=""btn-secondary"" onclick=""removeNewClassRow(this)"">Remove</button></td>' +
        '</tr>';
    }
})();";
            ScriptManager.RegisterStartupScript(this, GetType(), "resetClassForm", resetJs, true);
        }

        protected void gvFlights_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvFlights.EditIndex = -1;
            gvFlights.PageIndex = e.NewPageIndex;
            gvFlights.DataSource = ViewState["FlightsTable"];
            gvFlights.DataBind();
        }

        protected void gvFlights_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvFlights.EditIndex = e.NewEditIndex;
            LoadFlights();
        }

        protected void gvFlights_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvFlights.EditIndex = -1;
            LoadFlights();
        }

        protected void gvFlights_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = gvFlights.Rows[e.RowIndex];
            int flightId = Convert.ToInt32(gvFlights.DataKeys[e.RowIndex].Value);

            string flightNumber = ((TextBox)row.FindControl("txtEditFlightNumber")).Text.Trim();
            string departureText = ((TextBox)row.FindControl("txtEditDeparture")).Text;
            string arrivalText = ((TextBox)row.FindControl("txtEditArrival")).Text;

            DropDownList ddlEditSource = (DropDownList)row.FindControl("ddlEditSource");
            DropDownList ddlEditDestination = (DropDownList)row.FindControl("ddlEditDestination");
            DropDownList ddlEditAircraft = (DropDownList)row.FindControl("ddlEditAircraft");

            if (string.IsNullOrEmpty(flightNumber))
            {
                lblMessage.Text = "⚠ Flight number required for update!";
                return;
            }

            if (!DateTime.TryParse(departureText, out DateTime departure))
            {
                lblMessage.Text = "⚠ Invalid departure date/time format!";
                return;
            }
            if (!DateTime.TryParse(arrivalText, out DateTime arrival))
            {
                lblMessage.Text = "⚠ Invalid arrival date/time format!";
                return;
            }
            if (departure >= arrival)
            {
                lblMessage.Text = "⚠ Departure time must be before Arrival time!";
                return;
            }

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(@"
                    UPDATE Flights
                    SET FlightNumber=@FN, SourceAirportID=@SRC, DestinationAirportID=@DST,
                        DepartureTime=@DEP, ArrivalTime=@ARR, AircraftID=@AID
                    WHERE FlightID=@FID", con);

                cmd.Parameters.AddWithValue("@FN", flightNumber);
                cmd.Parameters.AddWithValue("@SRC", ddlEditSource.SelectedValue);
                cmd.Parameters.AddWithValue("@DST", ddlEditDestination.SelectedValue);
                cmd.Parameters.AddWithValue("@DEP", departure);
                cmd.Parameters.AddWithValue("@ARR", arrival);
                cmd.Parameters.AddWithValue("@AID", ddlEditAircraft.SelectedValue);
                cmd.Parameters.AddWithValue("@FID", flightId);

                cmd.ExecuteNonQuery();
            }

            gvFlights.EditIndex = -1;
            LoadFlights();

            lblMessage.Text = "✏️ Flight Updated!";
        }

        protected void gvFlights_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int flightId = Convert.ToInt32(gvFlights.DataKeys[e.RowIndex].Value);

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();

                SqlCommand deleteClassesCmd = new SqlCommand("DELETE FROM FlightClasses WHERE FlightID=@FID", con);
                deleteClassesCmd.Parameters.AddWithValue("@FID", flightId);
                deleteClassesCmd.ExecuteNonQuery();

                SqlCommand cmd = new SqlCommand("DELETE FROM Flights WHERE FlightID=@FID", con);
                cmd.Parameters.AddWithValue("@FID", flightId);
                cmd.ExecuteNonQuery();
            }

            LoadFlights();
            lblMessage.Text = "🗑 Flight Deleted Successfully!";
        }

        #endregion

        #region --- FlightClasses nested GridView (Class Price Management) ---

        private void BindFlightClassesGrid(int flightId, GridView grid)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT FlightClassID, FlightID, ClassName, BasePrice,
                           PriceMultiplier, SeatsAvailable
                    FROM FlightClasses WHERE FlightID = @FlightID ORDER BY FlightClassID";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.SelectCommand.Parameters.AddWithValue("@FlightID", flightId);

                DataTable dt = new DataTable();
                da.Fill(dt);

                grid.DataSource = dt;
                grid.DataBind();
            }
        }

        protected void gvFlights_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int currentFlightId = Convert.ToInt32(gvFlights.DataKeys[e.Row.RowIndex].Value);

                // 1. Bind the nested FlightClasses GridView
                GridView gvFlightClasses = (GridView)e.Row.FindControl("gvFlightClasses");
                if (gvFlightClasses != null)
                {
                    int flightId = currentFlightId;
                    BindFlightClassesGrid(flightId, gvFlightClasses);
                }

                // 2. Bind dropdowns for the main flight editing row
                if (gvFlights.EditIndex == e.Row.RowIndex)
                {
                    DataRowView drv = (DataRowView)e.Row.DataItem;

                    DataTable airportDt = GetAirports();

                    DropDownList ddlEditSource = (DropDownList)e.Row.FindControl("ddlEditSource");
                    if (ddlEditSource != null)
                    {
                        ddlEditSource.DataSource = airportDt;
                        ddlEditSource.DataTextField = "Display";
                        ddlEditSource.DataValueField = "AirportID";
                        ddlEditSource.DataBind();
                        string currentSourceId = drv["SourceAirportID"].ToString();
                        ddlEditSource.SelectedValue = currentSourceId;
                    }

                    DropDownList ddlEditDestination = (DropDownList)e.Row.FindControl("ddlEditDestination");
                    if (ddlEditDestination != null)
                    {
                        ddlEditDestination.DataSource = airportDt;
                        ddlEditDestination.DataTextField = "Display";
                        ddlEditDestination.DataValueField = "AirportID";
                        ddlEditDestination.DataBind();
                        string currentDestId = drv["DestinationAirportID"].ToString();
                        ddlEditDestination.SelectedValue = currentDestId;
                    }

                    DropDownList ddlEditAircraft = (DropDownList)e.Row.FindControl("ddlEditAircraft");
                    if (ddlEditAircraft != null)
                    {
                        ddlEditAircraft.DataSource = GetAircrafts();
                        ddlEditAircraft.DataTextField = "Display";
                        ddlEditAircraft.DataValueField = "AircraftID";
                        ddlEditAircraft.DataBind();
                        string currentAircraftId = drv["AircraftID"].ToString();
                        ddlEditAircraft.SelectedValue = currentAircraftId;
                    }
                }
            }
        }

        protected void gvFlightClasses_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView grid = (GridView)sender;
            grid.EditIndex = -1;
            grid.PageIndex = e.NewPageIndex;

            GridViewRow parentRow = (GridViewRow)grid.NamingContainer;
            int flightId = Convert.ToInt32(gvFlights.DataKeys[parentRow.RowIndex].Value);
            BindFlightClassesGrid(flightId, grid);
        }

        protected void gvFlightClasses_RowEditing(object sender, GridViewEditEventArgs e)
        {
            GridView grid = (GridView)sender;
            grid.EditIndex = e.NewEditIndex;

            GridViewRow parentRow = (GridViewRow)grid.NamingContainer;
            int flightId = Convert.ToInt32(gvFlights.DataKeys[parentRow.RowIndex].Value);

            BindFlightClassesGrid(flightId, grid);
        }

        protected void gvFlightClasses_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            GridView grid = (GridView)sender;
            grid.EditIndex = -1;

            GridViewRow parentRow = (GridViewRow)grid.NamingContainer;
            int flightId = Convert.ToInt32(gvFlights.DataKeys[parentRow.RowIndex].Value);

            BindFlightClassesGrid(flightId, grid);
        }

        protected void gvFlightClasses_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridView grid = (GridView)sender;
            GridViewRow row = grid.Rows[e.RowIndex];

            int flightClassId = Convert.ToInt32(grid.DataKeys[e.RowIndex].Value);

            TextBox txtClassName = (TextBox)row.FindControl("txtClassName");
            TextBox txtBasePrice = (TextBox)row.FindControl("txtBasePrice");
            TextBox txtPriceMultiplier = (TextBox)row.FindControl("txtPriceMultiplier");
            TextBox txtSeatsAvailable = (TextBox)row.FindControl("txtSeatsAvailable");

            if (string.IsNullOrWhiteSpace(txtClassName.Text)
                || !decimal.TryParse(txtBasePrice.Text, out decimal basePrice)
                || !decimal.TryParse(txtPriceMultiplier.Text, out decimal priceMul)
                || !int.TryParse(txtSeatsAvailable.Text, out int seats))
            {
                lblMessage.Text = "⚠ Invalid input in flight class! Check class name, price, multiplier, and seats.";
                return;
            }

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(@"
                    UPDATE FlightClasses
                    SET ClassName=@Name, BasePrice=@Base, PriceMultiplier=@Mul, SeatsAvailable=@Seats
                    WHERE FlightClassID=@ID", con);

                cmd.Parameters.AddWithValue("@Name", txtClassName.Text.Trim());
                cmd.Parameters.AddWithValue("@Base", basePrice);
                cmd.Parameters.AddWithValue("@Mul", priceMul);
                cmd.Parameters.AddWithValue("@Seats", seats);
                cmd.Parameters.AddWithValue("@ID", flightClassId);

                cmd.ExecuteNonQuery();
            }

            grid.EditIndex = -1;

            GridViewRow parentRow = (GridViewRow)grid.NamingContainer;
            int flightId = Convert.ToInt32(gvFlights.DataKeys[parentRow.RowIndex].Value);

            BindFlightClassesGrid(flightId, grid);
            lblMessage.Text = "✏️ Flight class updated!";
        }

        protected void gvFlightClasses_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            GridView grid = (GridView)sender;
            int flightClassId = Convert.ToInt32(grid.DataKeys[e.RowIndex].Value);

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM FlightClasses WHERE FlightClassID=@ID", con);
                cmd.Parameters.AddWithValue("@ID", flightClassId);
                cmd.ExecuteNonQuery();
            }

            GridViewRow parentRow = (GridViewRow)grid.NamingContainer;
            int flightId = Convert.ToInt32(gvFlights.DataKeys[parentRow.RowIndex].Value);

            BindFlightClassesGrid(flightId, grid);
            lblMessage.Text = "🗑 Flight class deleted!";
        }

        protected void gvFlightClasses_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "AddNew")
            {
                GridView grid = (GridView)sender;
                GridViewRow footer = grid.FooterRow;

                TextBox txtClassName = (TextBox)footer.FindControl("txtNewClassName");
                TextBox txtBasePrice = (TextBox)footer.FindControl("txtNewBasePrice");
                TextBox txtPriceMultiplier = (TextBox)footer.FindControl("txtNewPriceMultiplier");
                TextBox txtSeatsAvailable = (TextBox)footer.FindControl("txtNewSeatsAvailable");

                if (string.IsNullOrWhiteSpace(txtClassName.Text)
                    || !decimal.TryParse(txtBasePrice.Text, out decimal basePrice)
                    || !decimal.TryParse(txtPriceMultiplier.Text, out decimal priceMul)
                    || !int.TryParse(txtSeatsAvailable.Text, out int seats))
                {
                    lblMessage.Text = "⚠ Invalid input for new flight class! Check all fields.";
                    return;
                }

                GridViewRow parentRow = (GridViewRow)grid.NamingContainer;
                int flightId = Convert.ToInt32(gvFlights.DataKeys[parentRow.RowIndex].Value);

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO FlightClasses (FlightID, ClassName, BasePrice, PriceMultiplier, SeatsAvailable)
                        VALUES (@FID, @Name, @Base, @Mul, @Seats)", con);

                    cmd.Parameters.AddWithValue("@FID", flightId);
                    cmd.Parameters.AddWithValue("@Name", txtClassName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Base", basePrice);
                    cmd.Parameters.AddWithValue("@Mul", priceMul);
                    cmd.Parameters.AddWithValue("@Seats", seats);

                    cmd.ExecuteNonQuery();
                }

                BindFlightClassesGrid(flightId, grid);

                txtClassName.Text = string.Empty;
                txtBasePrice.Text = string.Empty;
                txtPriceMultiplier.Text = string.Empty;
                txtSeatsAvailable.Text = string.Empty;

                lblMessage.Text = "✅ Flight class added!";
            }
        }

        protected void gvFlightClasses_RowDataBound(object sender, GridViewRowEventArgs e)
        {
        }

        #endregion

        #region --- Helpers for dropdown binding ---

        private DataTable GetAirports()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT AirportID, Name + ' (' + Code + ')' AS Display FROM Airports ORDER BY Name", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        private DataTable GetAircrafts()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT AircraftID, Name + ' (' + Model + ')' AS Display FROM Aircrafts ORDER BY Name", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        #endregion

        #region --- Clear form ---

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            lblMessage.Text = "";
        }

        #endregion
    }
}
