<%@ Page Title="Manage Flight Status" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true"
    CodeBehind="AdminManageFlightStatus.aspx.cs" Inherits="project01.WebForm19" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<style>
    body { background-color: #f4f6f9; font-family: 'Segoe UI', sans-serif; color: #212529; }
    .container { max-width: 1100px; margin: 40px auto; background: #fff; padding: 25px; border-radius: 10px;
                 box-shadow: 0 4px 10px rgba(0,0,0,0.1); }

    h2 { text-align: center; color: #007bff; margin-bottom: 25px; }

    .search-section { text-align: right; margin-bottom: 15px; }
    .search-box {
        padding: 7px; border: 1px solid #ccc; border-radius: 5px;
        width: 220px;
    }

    .btn { padding: 7px 12px; border: none; border-radius: 5px; color: white; cursor: pointer; }
    .btn-search { background-color: #007bff; }
    .btn-clear { background-color: #17a2b8; }
    .btn:hover { opacity: 0.9; }

    .table { width: 100%; border-collapse: collapse; margin-top: 15px; }
    .table th, .table td {
        border: 1px solid #dee2e6; padding: 10px; text-align: center;
    }
    .table th { background: #007bff; color: white; }
    .table tr:nth-child(even) { background-color: #f8f9fa; }

    .ddl-status, .txt-msg {
        padding: 5px; border: 1px solid #ccc; border-radius: 4px;
    }

    .message {
        text-align: center; margin-top: 15px;
        color: #28a745; font-weight: bold;
    }
   .pager td { padding: 6px; }
    .pager a, .pager span {
        padding: 6px 12px; border: 1px solid #007bff; border-radius: 5px;
        margin: 2px; text-decoration: none; color: #007bff;
    }
    .pager span { background: #007bff; color: white; }
</style>

<div class="container">

    <h2>✈ Manage Flight Status</h2>

    <div class="search-section">
        <asp:TextBox ID="txtSearchFlight" runat="server" CssClass="search-box"
            placeholder="Enter Flight No (e.g. GF313)" />
        <asp:Button ID="btnSearch" runat="server" Text="Search"
            CssClass="btn btn-search" OnClick="btnSearch_Click" />
        <asp:Button ID="btnShowAll" runat="server" Text="Show All"
            CssClass="btn btn-clear" OnClick="btnShowAll_Click" />
    </div>

    <asp:GridView ID="gvFlightStatus" runat="server" AutoGenerateColumns="False"
        CssClass="table" DataKeyNames="FlightID"
        AllowPaging="True" PageSize="5" PagerStyle-CssClass="pager"
        OnPageIndexChanging="gvFlightStatus_PageIndexChanging"
        OnRowEditing="gvFlightStatus_RowEditing"
        OnRowCancelingEdit="gvFlightStatus_RowCancelingEdit"
        OnRowUpdating="gvFlightStatus_RowUpdating">

        <Columns>

            <asp:BoundField DataField="FlightNumber" HeaderText="Flight No" ReadOnly="true" />
            <asp:BoundField DataField="FromAirport" HeaderText="From" ReadOnly="true" />
            <asp:BoundField DataField="ToAirport" HeaderText="To" ReadOnly="true" />
            <asp:BoundField DataField="DepartureTime" HeaderText="Departure"
                DataFormatString="{0:dd-MMM HH:mm}" ReadOnly="true" />
            <asp:BoundField DataField="ArrivalTime" HeaderText="Arrival"
                DataFormatString="{0:dd-MMM HH:mm}" ReadOnly="true" />

            <asp:TemplateField HeaderText="Status">
                <ItemTemplate><%# Eval("Status") %></ItemTemplate>
                <EditItemTemplate>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="ddl-status">
                        <asp:ListItem>On Time</asp:ListItem>
                        <asp:ListItem>Delayed</asp:ListItem>
                        <asp:ListItem>Cancelled</asp:ListItem>
                        <asp:ListItem>Departed</asp:ListItem>
                        <asp:ListItem>Landed</asp:ListItem>
                        <asp:ListItem>Diverted</asp:ListItem>
                    </asp:DropDownList>
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Message">
                <ItemTemplate><%# Eval("Message") %></ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtMessage" runat="server"
                        Text='<%# Bind("Message") %>' CssClass="txt-msg" Width="220px"></asp:TextBox>
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:CommandField ShowEditButton="True" EditText="✏ Edit"
                CancelText="❌ Cancel" UpdateText="💾 Save" />

        </Columns>
    </asp:GridView>

    <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>

</div>

</asp:Content>