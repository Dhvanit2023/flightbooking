<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="project01.WebForm1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="register-container p-4 shadow-sm rounded mx-auto my-5">
        <h2 class="text-center mb-4" style="color:black">Login</h2>

        <asp:Label ID="lblMessage" runat="server" CssClass="lbl-message text-danger mb-3 d-block" />

        <div class="mb-3">
            <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" Placeholder="Enter Username" />
        </div>

        <div class="mb-3">
            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" Placeholder="Enter Password" />
        </div>

        <!-- IMAGE CAPTCHA -->
        <div class="mb-3 text-center">

            <asp:Image ID="imgCaptcha" runat="server" Width="150" Height="60" CssClass="border rounded" />

            <asp:LinkButton ID="btnRefresh" runat="server" CssClass="btn btn-link" OnClick="btnRefresh_Click">
                🔄 Refresh
            </asp:LinkButton>

        </div>

        <!-- CAPTCHA INPUT -->
        <div class="mb-3">
            <asp:TextBox ID="txtCaptcha" runat="server" CssClass="form-control" Placeholder="Enter Captcha" />
        </div>

        <div class="d-grid mb-3">
            <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn btn-primary" OnClick="btnLogin_Click" />
        </div>

        <div class="text-center mb-3">
            <a href="ForgotPassword.aspx" class="text-decoration-none">Forgot Password?</a>
        </div>

        <hr />

        <div class="text-center mb-3">
            <span style="color:black">OR</span>
        </div>

        <div class="d-grid">
            <asp:Button ID="Button1" runat="server" Text="Register" CssClass="btn btn-success" OnClick="Button1_Click" />
        </div>
    </div>

    <style>
        .register-container {
            max-width: 400px;
            width: 90%;
            background-color: #f9f9f9;
        }

        .lbl-message {
            font-weight: 600;
        }
    </style>

</asp:Content>
