<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="project01.WebForm2" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>My Profile</h2>

    <asp:Label ID="lblMessage" runat="server" ForeColor="Green"></asp:Label>
    <asp:Label ID="lblError" runat="server" ForeColor="Red"></asp:Label>
    <asp:HiddenField ID="hfUserID" runat="server" />

    <!-- Profile Image -->
    <div style="text-align:center; margin-bottom:20px;">
        <asp:Image ID="imgProfile" runat="server" Width="150px" Height="150px" Style="border-radius:50%; border:1px solid #333;" />
    </div>

    <asp:FileUpload ID="fuProfileImage" runat="server" />
    <br /><br />
    Name:
    <asp:TextBox ID="txtName" runat="server" Placeholder="Full Name" CssClass="form-control"  /><br /><br />
    E-mail:
    <asp:TextBox ID="txtEmail" runat="server" Placeholder="Email" CssClass="form-control" ReadOnly="true" /><br /><br />
    Username:
    <asp:TextBox ID="txtUsername" runat="server" Placeholder="Username" CssClass="form-control" ReadOnly="true" /><br /><br />
    Phone.No:
    <asp:TextBox ID="txtPhone" runat="server" Placeholder="Phone" CssClass="form-control" ReadOnly="true" /><br /><br />
    Password:
    <div style="position:relative; display:inline-block;">
        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" ReadOnly="true" Text="********" TextMode="Password" />
        <span id="togglePassword" style="cursor:pointer; margin-left:-25px;">👁️</span>
        <br />
        <hr />
        <asp:Button ID="btnSave" runat="server" Text="Update Profile" CssClass="btn btn-primary" OnClick="btnSave_Click" />
    </div>

    <br /><br />
    <!-- User Sessions -->
    <h3>My Active Sessions</h3>
    <asp:Button ID="btnTerminateAll" runat="server" Text="Terminate All Sessions" CssClass="btn btn-danger" OnClick="btnTerminateAll_Click" />
    <br /><br />
    <asp:GridView ID="gvUserSessions" runat="server" AutoGenerateColumns="False" EmptyDataText="No sessions found" GridLines="Both">
        <Columns>
            <asp:BoundField DataField="LogID" HeaderText="Log ID" />
            <asp:BoundField DataField="IPAddress" HeaderText="IP Address" />
            <asp:BoundField DataField="LoginTime" HeaderText="Login Time" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
            <asp:BoundField DataField="LogoutTime" HeaderText="Logout Time" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
            <asp:BoundField DataField="Activity" HeaderText="Activity" />
        </Columns>
    </asp:GridView>

    <!-- Scripts -->
    <script>
        document.addEventListener("DOMContentLoaded", function () {
            // Password toggle
            const toggle = document.getElementById("togglePassword");
            const pwd = document.getElementById("<%= txtPassword.ClientID %>");
            let actualPassword = '<%= ViewState["Password"] != null ? ViewState["Password"].ToString() : "" %>';

            toggle.addEventListener("click", function () {
                if (pwd.type === "password") {
                    pwd.type = "text";
                    pwd.value = actualPassword;
                    toggle.textContent = "🙈";
                } else {
                    pwd.type = "password";
                    pwd.value = "********";
                    toggle.textContent = "👁️";
                }
            });

            // Confirm before terminating sessions
            const btnTerminate = document.getElementById("<%= btnTerminateAll.ClientID %>");
            btnTerminate.addEventListener("click", function (e) {
                if (!confirm("Are you sure you want to terminate all sessions?")) {
                    e.preventDefault();
                }
            });
        });
    </script>

</asp:Content>
