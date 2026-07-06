<%@ Page Title="My Bookings" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="MyBookings.aspx.cs" Inherits="project01.WebForm14" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        body {
            background-color: #f8f9fa;
            font-family: 'Segoe UI', sans-serif;
        }

        .container {
            background: white;
            padding: 25px;
            border-radius: 12px;
            box-shadow: 0 4px 10px rgba(0,0,0,0.1);
            margin: 40px auto;
            width: 95%;
            max-width: 1100px;
            transition: 0.3s ease-in-out;
        }

        .container:hover {
            transform: translateY(-3px);
            box-shadow: 0 6px 14px rgba(0,0,0,0.15);
        }

        h2 {
            text-align: center;
            color: #007bff;
            font-size: 28px;
            margin-bottom: 25px;
        }

        .btn {
            background: #007bff;
            color: white;
            border: none;
            padding: 8px 14px;
            border-radius: 6px;
            cursor: pointer;
            font-size: 15px;
            transition: 0.2s;
        }

        .btn:hover {
            background: #0056b3;
        }

        .home-btn {
            margin-bottom: 15px;
            background: #28a745;
        }

        .home-btn:hover {
            background: #218838;
        }

        .message {
            display: block;
            text-align: center;
            color: red;
            font-weight: bold;
            margin-bottom: 15px;
        }

        .table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 10px;
            font-size: 15px;
        }

        .table th {
            background-color: #007bff;
            color: white;
            padding: 12px;
            text-align: center;
        }

        .table td {
            padding: 10px;
            border-bottom: 1px solid #e9ecef;
            text-align: center;
        }

        .table tr:nth-child(even) {
            background-color: #f8f9fa;
        }

        .table tr:hover {
            background-color: #f1f1f1;
        }

        .pager {
            text-align: center;
            margin-top: 15px;
        }

        .pager a, .pager span {
            display: inline-block;
            margin: 0 4px;
            padding: 6px 12px;
            text-decoration: none;
            border: 1px solid #007bff;
            border-radius: 5px;
            font-weight: bold;
        }

        .pager a {
            color: #007bff;
        }

        .pager span {
            background-color: #007bff;
            color: white;
        }
    </style>

        <h2>My Flight Bookings</h2>

        <asp:Button ID="btnHome" runat="server" CssClass="btn home-btn" Text="Back to Home" OnClick="btnHome_Click" />
        <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>

    <asp:GridView ID="gvBookings" runat="server" AutoGenerateColumns="False" CssClass="table"
    AllowPaging="True" PageSize="5"
    OnPageIndexChanging="gvBookings_PageIndexChanging"
    OnRowCommand="gvBookings_RowCommand" GridLines="None" CellPadding="8"
    BorderColor="#ddd" BorderWidth="1px">

    <Columns>
        <asp:BoundField DataField="BookingID" HeaderText="Booking ID" />
        <asp:BoundField DataField="FlightNumber" HeaderText="Flight Number" />
        <asp:BoundField DataField="SourceAirport" HeaderText="From" />
        <asp:BoundField DataField="DestinationAirport" HeaderText="To" />
        <asp:BoundField DataField="BookingDate" HeaderText="Booking Date" />
        <asp:BoundField DataField="SeatsBooked" HeaderText="Seats" />
        <asp:BoundField DataField="FinalPrice" HeaderText="Amount (₹)" DataFormatString="{0:F2}" />
        <asp:BoundField DataField="PaymentMethod" HeaderText="Method" />
        <asp:BoundField DataField="PaymentStatus" HeaderText="Payment" />
        <asp:BoundField DataField="Status" HeaderText="Booking Status" />
        <asp:TemplateField HeaderText="Actions">
            <ItemTemplate>
                <asp:Button ID="btnView" runat="server" Text="View Tickets" CssClass="btn"
                    CommandName="ViewTickets" CommandArgument='<%# Eval("BookingID") %>' />
            </ItemTemplate>
        </asp:TemplateField>
    </Columns>

    <HeaderStyle BackColor="#007bff" ForeColor="White" Font-Bold="True" />
    <RowStyle BackColor="#ffffff" />
    <AlternatingRowStyle BackColor="#f2f2f2" />
    <PagerStyle CssClass="pager" />
</asp:GridView>

</asp:Content>
