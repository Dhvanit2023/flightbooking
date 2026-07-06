<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="AdminNotifications.aspx.cs" Inherits="project01.WebForm10" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Send Notification</h2>
    
    <asp:Label ID="lblMessage" runat="server" ForeColor="Green"></asp:Label>
    
    <label>Title</label>
    <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" />

    <label>Message</label>
    <asp:TextBox ID="txtMessage" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" />

    <label>URL (Optional)</label>
    <asp:TextBox ID="txtURL" runat="server" CssClass="form-control" />

    <label>Select User (Optional)</label>
    <asp:DropDownList ID="ddlUsers" runat="server" CssClass="form-control">
        <asp:ListItem Text="--Select User--" Value="" />
    </asp:DropDownList>

    <asp:CheckBox ID="chkAllUsers" runat="server" Text="Send to all users" Checked="true" AutoPostBack="true" OnCheckedChanged="chkAllUsers_CheckedChanged" />

    <asp:Button ID="btnSend" runat="server" Text="Send Notification" CssClass="btn btn-primary" OnClick="btnSend_Click" />
</asp:Content>
