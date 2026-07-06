<%@ Page Title="Cancel Booking" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="CancelBooking.aspx.cs" Inherits="project01.WebForm16" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .cancel-box {
            width: 520px;
            margin: 50px auto;
            background: #fff;
            padding: 30px;
            border-radius: 12px;
            box-shadow: 0 0 15px rgba(0,0,0,0.1);
            text-align: center;
            font-family: 'Segoe UI', sans-serif;
        }
        .cancel-box h2 {
            color: #dc3545;
            margin-bottom: 20px;
        }
        .cancel-box input {
            width: 90%;
            padding: 10px;
            margin: 10px 0;
            border-radius: 8px;
            border: 1px solid #ccc;
            font-size: 16px;
        }
        .btn {
            background-color: #007bff;
            color: #fff;
            border: none;
            padding: 10px 18px;
            border-radius: 8px;
            cursor: pointer;
            font-size: 16px;
        }
        .btn:hover {
            background-color: #0056b3;
        }
        .btn-cancel {
            background-color: #dc3545;
        }
        .btn-cancel:hover {
            background-color: #b52a37;
        }
        .msg {
            color: green;
            font-weight: bold;
            margin-top: 15px;
            display: block;
        }
        .err {
            color: red;
            font-weight: bold;
            margin-top: 15px;
            display: block;
        }
        .otp-panel {
            margin-top: 20px;
            border-top: 1px solid #ccc;
            padding-top: 15px;
        }
    </style>

    <div class="cancel-box">
        <h2>Cancel My Booking</h2>

        <asp:Label ID="lblStep" runat="server" Text="Enter your Booking ID and Payment ID:" Font-Bold="true" />
        <br />
        <asp:TextBox ID="txtBookingID" runat="server" Placeholder="Enter Booking ID"></asp:TextBox><br />
        <asp:TextBox ID="txtPaymentID" runat="server" Placeholder="Enter Payment ID"></asp:TextBox><br />

        <asp:Button ID="btnSendOtp" runat="server" Text="Send OTP to Email" CssClass="btn" OnClick="btnSendOtp_Click" />

        <asp:Panel ID="pnlVerify" runat="server" CssClass="otp-panel" Visible="false">
            <asp:Label ID="lblOtpMsg" runat="server" Text="Enter the OTP sent to your registered email:" Font-Bold="true" />
            <br />
            <asp:TextBox ID="txtOtp" runat="server" Placeholder="Enter OTP"></asp:TextBox>
            <br />
            <asp:Button ID="btnVerifyOtp" runat="server" Text="Verify & Cancel Booking" CssClass="btn btn-cancel" OnClick="btnVerifyOtp_Click" />
        </asp:Panel>

        <asp:Label ID="lblMessage" runat="server" CssClass="msg" />
        <asp:Label ID="lblError" runat="server" CssClass="err" />
    </div>

</asp:Content>
