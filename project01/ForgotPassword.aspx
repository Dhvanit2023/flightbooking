<%@ Page Title="Forgot Password" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="project01.WebForm5" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div style="max-width:400px;margin:40px auto;padding:25px;border:1px solid #ccc;border-radius:10px;">
        <h3 class="text-center">Forgot Password</h3>

        <!-- Step 1: Enter Email -->
        <asp:Panel ID="pnlEmail" runat="server">
            <asp:Label ID="lblEmail" runat="server" Text="Enter Email:" /><br />
            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" /><br />
            <asp:Button ID="btnSendOTP" runat="server" Text="Send OTP" CssClass="btn btn-primary" OnClick="btnSendOTP_Click" /><br />
        </asp:Panel>

        <!-- Step 2: Verify OTP -->
        <asp:Panel ID="pnlOTP" runat="server" Visible="false">
            <asp:Label ID="lblOTP" runat="server" Text="Enter OTP sent to your email:" /><br />
            <asp:TextBox ID="txtOTP" runat="server" CssClass="form-control" /><br />
            <asp:Button ID="btnVerifyOTP" runat="server" Text="Verify OTP" CssClass="btn btn-success" OnClick="btnVerifyOTP_Click" /><br />
        </asp:Panel>

        <!-- Step 3: Reset Password -->
        <asp:Panel ID="pnlResetPassword" runat="server" Visible="false">
            <asp:Label ID="lblNew" runat="server" Text="New Password:" /><br />
            <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-control" TextMode="Password" onkeyup="checkStrength(this.value)" /><br />
            <div id="strengthMessage" style="font-weight:bold;"></div><br />
            <asp:Label ID="lblConfirm" runat="server" Text="Re-enter Password:" /><br />
            <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" TextMode="Password" /><br />
            <asp:Button ID="btnResetPassword" runat="server" Text="Reset Password" CssClass="btn btn-warning" OnClick="btnResetPassword_Click" /><br />
        </asp:Panel>

        <br />
        <asp:Label ID="lblMessage" runat="server" Text="" />
    </div>

    <script>
        function checkStrength(password) {
            const msg = document.getElementById("strengthMessage");
            if (password.length === 0) {
                msg.textContent = "";
                return;
            }
            let strength = 0;
            if (password.length >= 8) strength++;
            if (/[A-Z]/.test(password)) strength++;
            if (/[0-9]/.test(password)) strength++;
            if (/[^A-Za-z0-9]/.test(password)) strength++;

            if (strength <= 1) {
                msg.textContent = "Weak password";
                msg.style.color = "red";
            } else if (strength === 2) {
                msg.textContent = "Medium strength";
                msg.style.color = "blue";
            } else {
                msg.textContent = "Strong password";
                msg.style.color = "green";
            }
        }
    </script>
</asp:Content>
