<%@ Page Title="Confirm Booking" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Flight_Booking_Page1.aspx.cs" Inherits="project01.Flight_Booking_Page1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .confirm-box { max-width:700px; margin:40px auto; background:#0b2433; color:#fff; padding:20px; border-radius:10px; }
        .row { display:flex; gap:12px; margin-bottom:8px; border-bottom: 1px dashed rgba(255,255,255,0.1); padding-bottom: 5px; }
        .label { width:220px; font-weight:600; color:#cfe8ff; }
        .value { flex:1; }
        .total-price { font-size: 20px; font-weight: bold; color: #ffea00; margin-top: 15px; text-align: right; }
        .btn-confirm { padding:10px 18px; background:#28a745; color:#fff; border:none; border-radius:6px; cursor:pointer; font-size: 16px; }
    </style>

    <div class="confirm-box">
        <h2>📘 Confirm Booking</h2>

        <div class="row"><div class="label">Flight ID / Flight Class ID</div><div class="value"><asp:Label ID="lblFlightID" runat="server" /></div></div>
        <div class="row"><div class="label">From</div><div class="value"><asp:Label ID="lblFrom" runat="server" /></div></div>
        <div class="row"><div class="label">To</div><div class="value"><asp:Label ID="lblTo" runat="server" /></div></div>
        <div class="row"><div class="label">Class</div><div class="value"><asp:Label ID="lblClass" runat="server" /></div></div>
        <div class="row"><div class="label">Departure Date</div><div class="value"><asp:Label ID="lblDepartDate" runat="server" /></div></div>
        <div class="row"><div class="label">Passengers (A/C/S)</div><div class="value"><asp:Label ID="lblPassengers" runat="server" /></div></div>
        <div class="row"><div class="label">Base Fare (per adult)</div><div class="value"><asp:Label ID="lblBasePrice" runat="server" /></div></div>
        
        <div class="row"><div class="label">loyalty discount </div><div class="value"><asp:Label ID="l_discount" runat="server" /></div></div>
        
        <div class="total-price">Total Price: <asp:Label ID="lblFinalPrice" runat="server" Text="₹0" /></div>

        <div style="text-align:right; margin-top:14px;">
            <asp:Button ID="btnConfirm" runat="server" Text="✅ Confirm Booking" CssClass="btn-confirm" OnClick="btnConfirm_Click" />
        </div>
    </div>
</asp:Content>