<%@ Page Title="Admin - User Logs" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true"
    CodeBehind="AdminUserLogs.aspx.cs" Inherits="project01.AdminUserLogs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<h2>User Logs</h2>

<style>
    table { width: 100%; border-collapse: collapse; }
    th, td { padding: 8px; border: 1px solid #ddd; text-align: left; }
    th { background-color: #0078d7; color: white; }

    /* ⭐ Paging Design */
    .pager a, .pager span {
        padding: 6px 12px;
        margin: 2px;
        border: 1px solid #0078d7;
        color: #0078d7;
        border-radius: 5px;
        text-decoration: none;
        font-size: 14px;
    }

    .pager span {
        background: #0078d7;
        color: white;
        font-weight: bold;
    }

    .pager a:hover {
        background: #0078d7;
        color: white;
    }
</style>

<asp:GridView 
    ID="gvUserLogs" 
    runat="server" 
    AutoGenerateColumns="False" 
    EmptyDataText="No logs found"
    GridLines="Both"
    AllowPaging="True"
    PageSize="15"
    OnPageIndexChanging="gvUserLogs_PageIndexChanging"
    PagerSettings-Mode="NumericFirstLast"
    PagerSettings-PageButtonCount="5"
    PagerSettings-FirstPageText="<<"
    PagerSettings-LastPageText=">>"
    PagerSettings-NextPageText=">"
    PagerSettings-PreviousPageText="<">

  
    <PagerStyle CssClass="pager" HorizontalAlign="Center" />

    <Columns>
        <asp:BoundField DataField="LogID" HeaderText="Log ID" />
        <asp:BoundField DataField="UserName" HeaderText="User Name" />
        <asp:BoundField DataField="IPAddress" HeaderText="IP Address" />
        <asp:BoundField DataField="LoginTime" HeaderText="Login Time" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
        <asp:BoundField DataField="LogoutTime" HeaderText="Logout Time" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
        <asp:BoundField DataField="Activity" HeaderText="Activity" />
    </Columns>

</asp:GridView>

</asp:Content>
