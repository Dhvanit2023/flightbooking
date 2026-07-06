<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="PaymentSuccess.aspx.cs" Async="true" Inherits="project01.WebForm12" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div style="max-width:700px;margin:50px auto;padding:30px;border:1px solid #ccc;border-radius:8px;text-align:center;">
        <h2 style="color:green;">✅ Payment Successful!</h2>
        <p>Your booking and payment have been confirmed.</p>
        <hr />

        <table style="margin:auto;text-align:left;">
            <tr><td><b>Booking ID:</b></td><td><asp:Label ID="lblBookingId" runat="server" Text="-"></asp:Label></td></tr>
            <tr><td><b>Payment ID:</b></td><td><asp:Label ID="lblPaymentId" runat="server" Text="-"></asp:Label></td></tr>
            <tr><td><b>Amount Paid:</b></td><td><asp:Label ID="lblAmount" runat="server" Text="0.00"></asp:Label></td></tr>
            <tr><td><b>Date:</b></td><td><asp:Label ID="lblDate" runat="server" Text="-"></asp:Label></td></tr>
            <tr><td><b>Status:</b></td><td><asp:Label ID="lblStatus" runat="server" Text="Confirmed" ForeColor="Green"></asp:Label></td></tr>
        </table>

        <br /><br />
        <asp:Button ID="btnViewBookings" runat="server" Text="View My Bookings" CssClass="btn btn-primary" OnClick="btnViewBookings_Click" />
        &nbsp;
        <asp:Button ID="btnHome" runat="server" Text="Go to Home" CssClass="btn btn-secondary" OnClick="btnHome_Click" />
        <br /><br />
        <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>
        <br />
        <asp:Button ID="Button1" runat="server" Text="Ticket Downlod" OnClick="Button1_Click" />
    </div>
</asp:Content>
