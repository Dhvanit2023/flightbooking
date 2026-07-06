<%@ Page Title="Confirm Email" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="ConfirmEmail.aspx.cs" Inherits="project01.WebForm8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .message-box {
            max-width: 500px;
            margin: 60px auto;
            padding: 30px;
            border: 1px solid #ccc;
            background-color: #fff;
            border-radius: 10px;
            box-shadow: 0 0 10px #ddd;
            font-family: Arial, sans-serif;
            text-align: center;
        }

        .message-box h2 {
            margin-bottom: 20px;
        }

        .success {
            color: green;
        }

        .error {
            color: red;
        }
    </style>

    <div class="message-box">
        <h2>Email Verification</h2>
        <asp:Label ID="lblMessage" runat="server" CssClass="success"></asp:Label>
    </div>
</asp:Content>
