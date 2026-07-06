<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true"
    CodeBehind="AdminFlightStatistics.aspx.cs" Inherits="project01.WebForm22" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<style>
    body { background-color: #f4f6f9; font-family: 'Segoe UI', sans-serif; color: #212529; }
    .container { max-width: 900px; margin: 40px auto; background: #fff; padding: 30px; border-radius: 12px; box-shadow: 0 4px 10px rgba(0,0,0,0.1); }
    h2 { text-align: center; color: #007bff; margin-bottom: 25px; }
    .form { text-align: center; margin-bottom: 25px; }
    .form select, .form input, .form button {
        padding: 10px 15px; border: 1px solid #ccc; border-radius: 6px; font-size: 15px;
    }
    .form button {
        background: linear-gradient(90deg, #0072ff, #00c6ff); border: none; color: white;
        cursor: pointer; margin-left: 10px; transition: 0.3s;
    }
    .form button:hover { transform: scale(1.05); }

    .stats { display: flex; flex-wrap: wrap; justify-content: center; gap: 20px; margin-top: 30px; }
    .card {
        flex: 1 1 180px; text-align: center; background: #fff; padding: 20px;
        border-radius: 10px; border: 1px solid #dee2e6;
        box-shadow: 0 3px 10px rgba(0,0,0,0.08);
    }
    .card h3 { color: #007bff; font-size: 18px; margin-bottom: 10px; }
    .card p { font-size: 22px; font-weight: bold; color: #212529; margin: 0; }

    .table { width: 100%; margin-top: 25px; border-collapse: collapse; font-size: 15px; }
    .table th, .table td { border: 1px solid #dee2e6; padding: 10px; text-align: center; }
    .table th { background: #007bff; color: white; }
    .message { text-align: center; color: #dc3545; font-weight: bold; margin-top: 15px; }

    .pager td { padding: 6px; }
    .pager a, .pager span {
        padding: 6px 12px; border: 1px solid #007bff; border-radius: 5px; margin: 2px;
        text-decoration: none; color: #007bff;
    }
    .pager span { background: #007bff; color: white; }
</style>

<div class="container">

    <h2>📊 Flight Statistics Dashboard</h2>

    <div class="form">
        <asp:DropDownList ID="ddlFlight" runat="server" Width="250px"></asp:DropDownList>
        <asp:Button ID="btnViewStats" runat="server" Text="View Statistics" OnClick="btnViewStats_Click" />
    </div>

    <!-- Flight Overview -->
    <asp:Panel ID="pnlStats" runat="server" Visible="false">

        <h3 style="text-align:center; color:#007bff;">✈ Flight Overview</h3>

        <table class="table">
            <tr>
                <th>Flight Number</th>
                <th>Route</th>
                <th>Departure</th>
                <th>Arrival</th>
            </tr>
            <tr>
                <td><asp:Label ID="lblFlightNumber" runat="server" /></td>
                <td><asp:Label ID="lblRoute" runat="server" /></td>
                <td><asp:Label ID="lblDeparture" runat="server" /></td>
                <td><asp:Label ID="lblArrival" runat="server" /></td>
            </tr>
        </table>

        <div class="stats">
            <div class="card"><h3>Total Seats</h3><p><asp:Label ID="lblTotalSeats" runat="server" /></p></div>
            <div class="card"><h3>Booked Seats</h3><p><asp:Label ID="lblBookedSeats" runat="server" /></p></div>
            <div class="card"><h3>Confirmed Seats</h3><p><asp:Label ID="lblConfirmedSeats" runat="server" /></p></div>
            <div class="card"><h3>Cancelled Seats</h3><p style="color:#dc3545;"><asp:Label ID="lblCancelledSeats" runat="server" /></p></div>
            <div class="card"><h3>Available Seats</h3><p style="color:#28a745;"><asp:Label ID="lblAvailableSeats" runat="server" /></p></div>
        </div>

        <h3 style="text-align:center; margin-top:30px;">🧾 Passenger Booking Summary</h3>

        <!-- GridView With Paging -->
        <asp:GridView ID="gvBookings" runat="server" CssClass="table" AutoGenerateColumns="False"
            AllowPaging="True" PageSize="5" PagerStyle-CssClass="pager"
            OnPageIndexChanging="gvBookings_PageIndexChanging">

            <Columns>
                <asp:BoundField DataField="BookingID" HeaderText="Booking ID" />
                <asp:BoundField DataField="UserName" HeaderText="User" />
                <asp:BoundField DataField="SeatsBooked" HeaderText="Seats" />
                <asp:BoundField DataField="Status" HeaderText="Status" />
                <asp:BoundField DataField="BookingDate" HeaderText="Date" DataFormatString="{0:dd-MMM-yyyy}" />
            </Columns>

        </asp:GridView>

    </asp:Panel>

    <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>

</div>

</asp:Content>
