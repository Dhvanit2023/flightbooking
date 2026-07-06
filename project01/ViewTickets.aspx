<%@ Page Title="GoFly E-Ticket" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="ViewTickets.aspx.cs" Inherits="project01.WebForm15" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .ticket-container {
            background: #fff;
            padding: 30px;
            border-radius: 12px;
            box-shadow: 0 0 15px rgba(0,0,0,0.1);
            width: 850px;
            margin: 30px auto;
            font-family: Arial, sans-serif;
        }
        h2 { text-align: center; color: #007bff; margin-bottom: 10px; }
        .section-title { background-color: #007bff; color: white; padding: 5px 10px; font-weight: bold; margin-top: 15px; }
        .ticket-table { width: 100%; border-collapse: collapse; margin-top: 10px; }
        .ticket-table th, .ticket-table td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        .ticket-table th { background-color: #f8f9fa; }
        .download-btn {
            display: block; width: 220px; margin: 20px auto;
            background: #28a745; color: white; border: none;
            padding: 10px 20px; border-radius: 8px; cursor: pointer;
            text-align: center;
        }
        .download-btn:hover { background: #218838; }
        p { margin: 3px 0; }
    </style>

    <div class="ticket-container">
        <h2>GoFly E-Ticket</h2>
        <hr />
        <p><strong>Booking ID:</strong> <asp:Label ID="lblBookingId" runat="server" /></p>
        <p><strong>Payment ID:</strong> <asp:Label ID="lblPaymentId" runat="server" /></p>
        <p><strong>Status:</strong> <asp:Label ID="lblStatus" runat="server" /></p>
        <p><strong>Date:</strong> <asp:Label ID="lblDate" runat="server" /></p>
        <p><strong>Amount:</strong> <asp:Label ID="lblAmount" runat="server" /></p>
        <hr />
        <p><strong>From:</strong> <asp:Label ID="lblFrom" runat="server" /></p>
        <p><strong>To:</strong> <asp:Label ID="lblTo" runat="server" /></p>
        <p><strong>Flight No:</strong> <asp:Label ID="lblFlightNumber" runat="server" /></p>
        <p><strong>Departure:</strong> <asp:Label ID="lblDeparture" runat="server" /></p>
        <p><strong>Arrival:</strong> <asp:Label ID="lblArrival" runat="server" /></p>
        <p><strong>Total Passengers:</strong> <asp:Label ID="lblPassengers" runat="server" /></p>

        <div class="section-title">Passenger List</div>
        <asp:GridView ID="gvPassengers" runat="server" AutoGenerateColumns="True" CssClass="ticket-table" />

        <div class="section-title">Ticket Details</div>
        <asp:GridView ID="gvTicket" runat="server" AutoGenerateColumns="True" CssClass="ticket-table" />
        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" Placeholder="Enter email to send ticket" />
<asp:Button ID="Button1" runat="server" Text="Download & Send Ticket (PDF)" CssClass="download-btn" OnClick="btnDownload_Click" />
<asp:Label ID="Label1" runat="server" ForeColor="Red" />

        <asp:Label ID="lblMessage" runat="server" ForeColor="Red" />
        <asp:Button ID="btnDownload" runat="server" CssClass="download-btn" Text="Download Ticket (PDF)" OnClick="btnDownload_Click" Visible="false" />
        <asp:Button ID="btnViewBookings" runat="server" CssClass="download-btn" Text="View My Bookings" OnClick="btnViewBookings_Click" />
    </div>
</asp:Content>
