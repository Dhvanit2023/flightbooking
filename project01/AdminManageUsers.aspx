<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true"
    CodeBehind="AdminManageUsers.aspx.cs" Inherits="project01.WebForm21" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<style>
    body { background-color: #f4f6f9; font-family: 'Segoe UI', sans-serif; }
    .container {
        max-width: 1100px; margin: 40px auto; background: #fff; padding: 25px;
        border-radius: 10px; box-shadow: 0 4px 10px rgba(0,0,0,0.1);
    }
    h2 { text-align: center; color: #007bff; margin-bottom: 25px; }

    .search-bar { text-align: right; margin-bottom: 15px; }
    .search-box {
        padding: 7px; border: 1px solid #ccc; border-radius: 5px; width: 220px;
    }

    .btn { padding: 6px 10px; border: none; border-radius: 5px; color: white; cursor: pointer; }
    .btn-search { background: #007bff; }
    .btn-admin { background: #17a2b8; }
    .btn-verify { background: #28a745; }
    .btn-delete { background: #dc3545; }
    .btn:hover { opacity: 0.85; }

    .verified { color: #28a745; font-weight: bold; }
    .not-verified { color: #dc3545; font-weight: bold; }
    .admin { background-color: #e3f2fd; font-weight: bold; color: #007bff; }

    .message { text-align: center; margin-top: 10px; color: #28a745; font-weight: bold; }

    /* Paging design */
    .pager td { padding: 6px; }
    .pager a, .pager span {
        padding: 6px 12px; border: 1px solid #007bff; border-radius: 5px;
        margin: 2px; text-decoration: none; color: #007bff;
    }
    .pager span { background: #007bff; color: white; }
</style>

<div class="container">

    <h2>👥 Manage Users</h2>

    <div class="search-bar">
        <asp:TextBox ID="txtSearchEmail" runat="server" CssClass="search-box" placeholder="Search by Email..." />
        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-search" OnClick="btnSearch_Click" />
        <asp:Button ID="btnShowAll" runat="server" Text="Show All" CssClass="btn btn-admin" OnClick="btnShowAll_Click" />
    </div>

    <!-- GRID WITH PAGING -->
    <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="False" CssClass="table"
        AllowPaging="True" PageSize="6" PagerStyle-CssClass="pager"
        DataKeyNames="UserID"
        OnPageIndexChanging="gvUsers_PageIndexChanging"
        OnRowCommand="gvUsers_RowCommand">

        <Columns>

            <asp:BoundField DataField="UserID" HeaderText="User ID" />
            <asp:BoundField DataField="Name" HeaderText="Name" />
            <asp:BoundField DataField="Email" HeaderText="Email" />
            <asp:BoundField DataField="Phone" HeaderText="Phone" />
            <asp:BoundField DataField="CreatedAt" HeaderText="Joined On" DataFormatString="{0:dd-MMM-yyyy}" />

            <asp:TemplateField HeaderText="Verified">
                <ItemTemplate>
                    <span class='<%# (bool)Eval("IsVerified") ? "verified" : "not-verified" %>'>
                        <%# (bool)Eval("IsVerified") ? "Yes" : "No" %>
                    </span>
                </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Admin">
                <ItemTemplate>
                    <span class='<%# (bool)Eval("IsAdmin") ? "admin" : "" %>'>
                        <%# (bool)Eval("IsAdmin") ? "Yes" : "No" %>
                    </span>
                </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Actions">
                <ItemTemplate>
                    <asp:Button ID="btnVerify" runat="server" CssClass="btn btn-verify"
                        Text='<%# (bool)Eval("IsVerified") ? "Unverify" : "Verify" %>'
                        CommandName="ToggleVerify" CommandArgument='<%# Eval("UserID") %>' />

                    <asp:Button ID="btnAdmin" runat="server" CssClass="btn btn-admin"
                        Text='<%# (bool)Eval("IsAdmin") ? "Demote" : "Promote" %>'
                        CommandName="ToggleAdmin" CommandArgument='<%# Eval("UserID") %>' />

                    <asp:Button ID="btnDelete" runat="server" CssClass="btn btn-delete" Text="Delete"
                        CommandName="DeleteUser" CommandArgument='<%# Eval("UserID") %>' />
                </ItemTemplate>
            </asp:TemplateField>

        </Columns>

    </asp:GridView>

    <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>

</div>

</asp:Content>
