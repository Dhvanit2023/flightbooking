<%@ Page Title="Admin Dashboard" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="AdminDashboard.aspx.cs" Inherits="project01.WebForm17" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        /* Base Styles */
        body { background-color:#f4f6f9; font-family:'Segoe UI',sans-serif; color:#212529; }
        .dashboard-container { max-width:1200px; margin:40px auto; padding:25px; background:#fff; border-radius:12px;
            box-shadow:0 4px 12px rgba(0,0,0,0.08); }
        .header { text-align:center; font-size:30px; font-weight:bold; color:#007bff; margin-bottom:20px; }
        
        /* Stats Cards */
        .stats { display:flex; flex-wrap:wrap; gap:20px; justify-content:space-between; }
        .card { flex:1 1 220px; padding:18px; border-radius:10px; border:1px solid #dee2e6; text-align:center; cursor:pointer;
            background:#fff; box-shadow:0 2px 6px rgba(0,0,0,0.04); transition:0.2s; }
        .card h3 { margin:0; color:#007bff; font-size:18px; }
        .card p { font-size:24px; margin:8px 0 0; font-weight:700; color:#212529; }
        .card:hover { transform: translateY(-3px); box-shadow: 0 4px 10px rgba(0,0,0,0.1); }

        /* Report Sections & Tables */
        .profit-loss { display:none; margin-top:20px; padding:18px; border-radius:10px; background-color: #f8f9fa; border: 1px solid #e9ecef; }
        .section-title { font-size:20px; margin:28px 0 10px; color:#343a40; border-bottom:2px solid #007bff; padding-bottom:6px; }
        .table { width:100%; border-collapse:collapse; margin-bottom: 20px; }
        .table th, .table td { padding:10px 8px; border-bottom:1px solid #dee2e6; }
        .table th { text-align:left; background:#e9ecef; color:#495057; font-weight:600; }
        .table tr:last-child td { border-bottom: none; }
        
        /* Hot Sectors Grid styling */
        .sector-grid { margin-top: 10px; }
        .sector-grid th { background-color: #fff3cd; color: #856404; } /* Light yellow for hot sector headers */
        
        /* Filters and Buttons */
        .filters { display:flex; gap:12px; flex-wrap:wrap; margin:14px 0; align-items:center; }
        .small { font-size:13px; color:#6c757d; padding: 8px; border: 1px solid #ced4da; border-radius: 4px; }
        .link-buttons { margin-top:20px; display:flex; gap:12px; flex-wrap:wrap; justify-content:center; }
        .btn { padding:10px 14px; border-radius:8px; border:none; cursor:pointer; font-weight:600; background-color: #6c757d; color: white; transition: background-color 0.2s; }
        .btn:hover { background-color: #5a6268; }
        #btnApplyFilter { background-color: #28a745; }
        #btnApplyFilter:hover { background-color: #218838; }
        #btnExportCsv { background-color: #007bff; }
        #btnExportCsv:hover { background-color: #0056b3; }
    </style>

    <div class="dashboard-container">
        <div class="header">✈️ Admin Dashboard</div>

        <div class="stats">
            <div class="card" title="Total Flights">
                <h3>Total Flights</h3>
                <p><asp:Label ID="lblTotalFlights" runat="server" Text="0" /></p>
            </div>
            <div class="card" title="Total Users">
                <h3>Total Users</h3>
                <p><asp:Label ID="lblTotalUsers" runat="server" Text="0" /></p>
            </div>
            <div class="card" title="Total Bookings">
                <h3>Total Bookings</h3>
                <p><asp:Label ID="lblTotalBookings" runat="server" Text="0" /></p>
            </div>
            <div class="card" title="Cancelled Tickets">
                <h3>Cancelled Tickets</h3>
                <p style="color:#dc3545;"><asp:Label ID="lblCancelledTickets" runat="server" Text="0" /></p>
            </div>

            <asp:LinkButton ID="lnkRevenue" runat="server" CssClass="card" OnClick="lnkRevenue_Click">
                <h3>Total Revenue (₹)</h3>
                <p><asp:Label ID="lblTotalRevenue" runat="server" Text="0" /></p>
            </asp:LinkButton>
            <asp:LinkButton ID="LinkButton1" runat="server" CssClass="card" OnClick="lnkRevenue_Click">
    <h3>Net Revenue (₹)</h3>
    <p><asp:Label ID="lblNetProfitLoss2" runat="server" Text="0" /></p>
</asp:LinkButton>
                        <asp:LinkButton ID="LinkButton2" runat="server" CssClass="card" OnClick="lnkRevenue_Click">
    <h3>Total Refund (₹)</h3>
    <p><asp:Label ID="Label1" runat="server" Text="0" /></p>
</asp:LinkButton>

        </div>

        <div class="filters">
            <asp:DropDownList ID="ddlPeriod" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlPeriod_SelectedIndexChanged">
                <asp:ListItem Value="day">Day</asp:ListItem>
                <asp:ListItem Value="week" Selected="True">Week</asp:ListItem>
                <asp:ListItem Value="month">Month</asp:ListItem>
                <asp:ListItem Value="year">Year</asp:ListItem>
                <asp:ListItem Value="custom">Custom</asp:ListItem>
            </asp:DropDownList>

            <asp:TextBox ID="txtFrom" runat="server" CssClass="small" placeholder="From (yyyy-mm-dd)"></asp:TextBox>
            <asp:TextBox ID="txtTo" runat="server" CssClass="small" placeholder="To (yyyy-mm-dd)"></asp:TextBox>

            <asp:TextBox ID="txtUsername" runat="server" CssClass="small" placeholder="Username or Name"></asp:TextBox>

            <asp:DropDownList ID="ddlClass" runat="server" CssClass="small"></asp:DropDownList>
            <asp:DropDownList ID="ddlSource" runat="server" CssClass="small"></asp:DropDownList>
            <asp:DropDownList ID="ddlDestination" runat="server" CssClass="small"></asp:DropDownList>

            <asp:Button ID="btnApplyFilter" runat="server" Text="Apply Filter" CssClass="btn" OnClick="btnApplyFilter_Click" />
            <asp:Button ID="btnExportCsv" runat="server" Text="Export CSV" CssClass="btn" OnClick="btnExportCsv_Click" />
            <asp:Button ID="btnExportPdf" runat="server" Text="Export PDF" CssClass="btn" OnClick="btnExportPdf_Click" style="background-color: #dc3545;" />
        </div>

        <div id="divProfitLoss" runat="server" class="profit-loss">
            <h4>📊 Profit & Loss Summary (Filtered Period)</h4>
            <table class="table">
                <tr><th>Metric</th><th>Amount (₹)</th></tr>
                <tr><td>Total Revenue</td><td><asp:Label ID="lblRevenueDetails" runat="server" Text="0.00" /></td></tr>
                <tr><td>Total Operational Cost (Est.)</td><td><asp:Label ID="lblOperationalCost" runat="server" Text="0.00" /></td></tr>
                <tr style="font-weight: bold;"><td>Net Profit / Loss (Amount)</td><td><asp:Label ID="lblNetProfitLoss" runat="server" Text="0.00" /></td></tr>
                <tr><td>Net Profit / Loss (%)</td><td><asp:Label ID="lblProfitMargin" runat="server" Text="0%" /></td></tr>
            </table>
        </div>

        <div class="section-title">🔥 Top Booking Sectors</div>
        <asp:GridView ID="gvHotSectors" runat="server" AutoGenerateColumns="False" CssClass="table sector-grid" EmptyDataText="No high-volume routes found in this period.">
            <Columns>
                <asp:BoundField DataField="Route" HeaderText="Route (Source -> Destination)" />
                <asp:BoundField DataField="BookingsCount" HeaderText="Bookings Count" ItemStyle-HorizontalAlign="Right" />
            </Columns>
        </asp:GridView>

        <div class="section-title">🧾 Recent Bookings</div>
        <asp:GridView ID="gvRecentBookings" runat="server" AutoGenerateColumns="False" CssClass="table" EmptyDataText="No bookings found">
            <Columns>
                <asp:BoundField DataField="BookingID" HeaderText="Booking ID" />
                <asp:BoundField DataField="Username" HeaderText="User" />
                <asp:BoundField DataField="FlightNumber" HeaderText="Flight No" />
                <asp:BoundField DataField="ClassName" HeaderText="Class" />
                <asp:BoundField DataField="SeatsBooked" HeaderText="Seats" />
                <asp:BoundField DataField="FinalPrice" HeaderText="Amount (₹)" DataFormatString="{0:N2}" ItemStyle-HorizontalAlign="Right" />
                <asp:BoundField DataField="Status" HeaderText="Status" />
                <asp:BoundField DataField="BookingDate" HeaderText="Date" DataFormatString="{0:dd-MMM-yyyy HH:mm}" />
                <asp:BoundField DataField="SourceCity" HeaderText="From" />
                <asp:BoundField DataField="DestinationCity" HeaderText="To" />
            </Columns>
        </asp:GridView>

        <div class="link-buttons">
            <asp:Button ID="btnManageFlights" runat="server" Text="✈ Manage Flights" CssClass="btn" OnClick="btnManageFlights_Click" />
            <asp:Button ID="btnManageUsers" runat="server" Text="👥 Manage Users" CssClass="btn" OnClick="btnManageUsers_Click" />
            <asp:Button ID="btnManagePayments" runat="server" Text="💳 View Payments" CssClass="btn" OnClick="btnManagePayments_Click" />
            <asp:Button ID="btnSupport" runat="server" Text="💬 Support Chat" CssClass="btn" OnClick="btnSupport_Click" />
            <asp:Button ID="btnFlightStatus" runat="server" Text="🕓 Flight Status" CssClass="btn" OnClick="btnFlightStatus_Click" />
            <asp:Button ID="btnFlightStats" runat="server" Text="📈 Flight Statistics" CssClass="btn" OnClick="btnFlightStats_Click" />
            <asp:Button ID="btnDiscounts" runat="server" Text="🎟️ Discounts" CssClass="btn" OnClick="btnDiscounts_Click" />
            <asp:Button ID="btnHotDeals" runat="server" Text="🔥 Hot Deals" CssClass="btn" OnClick="btnHotDeals_Click" />
            <asp:Button ID="btnTravelNews" runat="server" Text="📰 Travel News" CssClass="btn" OnClick="btnTravelNews_Click" />
        </div>
    </div>
</asp:Content>