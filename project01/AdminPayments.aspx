<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" 
    CodeBehind="AdminPayments.aspx.cs" Inherits="project01.WebForm20" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<style>
    body {
        background-color: #f4f6f9;
        font-family: 'Segoe UI', sans-serif;
    }

    .container {
        max-width: 1100px;
        margin: 40px auto;
        background: #fff;
        padding: 25px;
        border-radius: 10px;
        box-shadow: 0 4px 10px rgba(0,0,0,0.1);
    }

    h2 {
        text-align: center;
        color: #007bff;
        margin-bottom: 25px;
    }

    .filters {
        display: flex;
        gap: 10px;
        margin-bottom: 15px;
    }

    input[type=text], select {
        padding: 7px;
        border: 1px solid #ccc;
        border-radius: 5px;
    }

    .btn-refresh {
        background: #007bff;
        color: white;
        padding: 7px 12px;
        border: none;
        border-radius: 5px;
        cursor: pointer;
    }

    .btn-refresh:hover {
        background: #0056b3;
    }

    .table {
        width: 100%;
        border-collapse: collapse;
    }

    .table th, .table td {
        border: 1px solid #dee2e6;
        padding: 10px;
        text-align: center;
    }

    .table th {
        background-color: #007bff;
        color: white;
    }

    .table tr:nth-child(even) {
        background-color: #f8f9fa;
    }

    .table tr:hover {
        background-color: #f1f1f1;
    }

    .status-paid { color: #28a745; font-weight: bold; }
    .status-pending { color: #ffc107; font-weight: bold; }
    .status-failed { color: #dc3545; font-weight: bold; }

    /* ⭐ PAGING FIX */
    .pager a, .pager span {
        padding: 6px 12px;
        margin: 2px;
        border: 1px solid #007bff;
        color: #007bff;
        border-radius: 5px;
        text-decoration: none;
    }

    .pager span {
        background: #007bff;
        color: white;
    }

    .pager a:hover {
        background: #007bff;
        color: white;
    }
</style>

<div class="container">
    <h2>💳 Payment Transactions</h2>

    <div class="filters">
        <asp:TextBox ID="txtSearchUser" runat="server" Placeholder="Search Username"></asp:TextBox>

        <asp:DropDownList ID="ddlStatusFilter" runat="server">
            <asp:ListItem Text="All" Value="All" />
            <asp:ListItem Text="Completed" Value="Completed" />
            <asp:ListItem Text="Pending" Value="Pending" />
            <asp:ListItem Text="Refund Initiated" Value="Refund Initiated" />
        </asp:DropDownList>

        <asp:DropDownList ID="ddlSort" runat="server">
            <asp:ListItem Text="Newest First" Value="DESC" />
            <asp:ListItem Text="Oldest First" Value="ASC" />
        </asp:DropDownList>

        <asp:Button ID="btnFilter" runat="server" Text="Apply" CssClass="btn-refresh" OnClick="btnFilter_Click" />
    </div>

    <asp:GridView ID="gvPayments" runat="server" AutoGenerateColumns="False" CssClass="table"
        AllowPaging="true" PageSize="10"
        OnPageIndexChanging="gvPayments_PageIndexChanging">

        
        <PagerStyle CssClass="pager" HorizontalAlign="Center" />

        <Columns>
            <asp:BoundField DataField="PaymentID" HeaderText="Payment ID" />
            <asp:BoundField DataField="BookingID" HeaderText="Booking ID" />
            <asp:BoundField DataField="UserName" HeaderText="User Name" />
            <asp:BoundField DataField="Amount" HeaderText="Amount (₹)" DataFormatString="{0:F2}" />
            <asp:BoundField DataField="PaymentMethod" HeaderText="Method" />
            <asp:BoundField DataField="PaymentDate" HeaderText="Date" DataFormatString="{0:dd-MMM-yyyy HH:mm}" />
            <asp:BoundField DataField="IPAddress" HeaderText="IP Address" />
            <asp:BoundField DataField="Region" HeaderText="Region" />

            <asp:TemplateField HeaderText="Status">
                <ItemTemplate>
                    <span class='<%# GetStatusCss(Eval("PaymentStatus").ToString()) %>'>
                        <%# Eval("PaymentStatus") %>
                    </span>
                </ItemTemplate>
            </asp:TemplateField>

            <asp:BoundField DataField="RazorpayPaymentID" HeaderText="Transaction Ref" />
        </Columns>
    </asp:GridView>

    <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>
</div>

</asp:Content>
