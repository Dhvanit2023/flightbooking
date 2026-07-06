<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="project01.Register" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="register-container">
        <h2>User Registration</h2>

        <!-- Name -->
        <label for="txtName" style="color:black">Name</label>
        <asp:TextBox ID="txtName" runat="server" placeholder="Enter your name" CssClass="form-control" />
        <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName" ErrorMessage="Name is required" ForeColor="Red" Display="Dynamic" />

        <!-- Email -->
        <label for="txtEmail" style="color:black">Email</label>
        <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" placeholder="Enter your email" CssClass="form-control" />
        <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" ErrorMessage="Email is required" ForeColor="Red" Display="Dynamic" />
        <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
            ValidationExpression="^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"
            ErrorMessage="Enter valid email" ForeColor="Red" Display="Dynamic" />

        <!-- Username -->
        <label for="txtUsername" style="color:black">Username</label>
        <asp:TextBox ID="txtUsername" runat="server" placeholder="Choose a username" CssClass="form-control" />
        <asp:RequiredFieldValidator ID="rfvUsername" runat="server" ControlToValidate="txtUsername" ErrorMessage="Username is required" ForeColor="Red" Display="Dynamic" />

        <!-- Password -->
        <label for="txtPassword" style="color:black">Password</label>
        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" placeholder="Enter password" CssClass="form-control" />
        <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" ErrorMessage="Password is required" ForeColor="Red" Display="Dynamic" />

        <div class="strength-bar">
            <div id="strengthBarInner"></div>
        </div>
        <div id="strengthText" class="strength-text"></div>

      
        <!-- Phone -->
        <label for="txtPhone" style="color:black">Phone Number</label>
        <asp:TextBox ID="txtPhone" runat="server" placeholder="+91 Phone Number" CssClass="form-control" />
        <asp:RequiredFieldValidator ID="rfvPhone" runat="server" ControlToValidate="txtPhone" ErrorMessage="Phone is required" ForeColor="Red" Display="Dynamic" />
        <asp:RegularExpressionValidator ID="revPhone" runat="server" ControlToValidate="txtPhone" 
            ErrorMessage="Enter valid phone number in format +911234567890" 
            ValidationExpression="^\+91\d{10}$" 
            ForeColor="Red" Display="Dynamic" />

        <!-- Register Button -->
        <asp:Button ID="btnRegister" runat="server" Text="Register" CssClass="btn-submit" OnClick="btnRegister_Click" />
        <br />
        <asp:Label ID="lblStatus" runat="server" CssClass="error-message" style="color:black" />
    </div>

    <script>
        function checkPasswordStrength(pwd) {
            const strengthBar = document.getElementById("strengthBarInner");
            const strengthText = document.getElementById("strengthText");

            let strength = 0;
            if (pwd.length >= 7 && pwd.length <= 16) strength++;
            if (/[A-Z]/.test(pwd)) strength++;
            if (/\d/.test(pwd)) strength++;
            const specialChars = pwd.match(/[^A-Za-z0-9]/g);
            if (specialChars && specialChars.length >= 2) strength += 2;
            else if (specialChars && specialChars.length === 1) strength += 1;

            switch (strength) {
                case 0:
                case 1:
                case 2:
                    strengthBar.style.width = "25%";
                    strengthBar.style.backgroundColor = "red";
                    strengthText.textContent = "Weak";
                    strengthText.style.color = "red";
                    break;
                case 3:
                    strengthBar.style.width = "50%";
                    strengthBar.style.backgroundColor = "orange";
                    strengthText.textContent = "Moderate";
                    strengthText.style.color = "orange";
                    break;
                case 4:
                    strengthBar.style.width = "75%";
                    strengthBar.style.backgroundColor = "blue";
                    strengthText.textContent = "Good";
                    strengthText.style.color = "blue";
                    break;
                case 5:
                    strengthBar.style.width = "100%";
                    strengthBar.style.backgroundColor = "green";
                    strengthText.textContent = "Strong";
                    strengthText.style.color = "green";
                    break;
                default:
                    strengthBar.style.width = "0%";
                    strengthText.textContent = "";
            }
        }

        window.onload = function () {
            const pwdInput = document.getElementById("<%= txtPassword.ClientID %>");
            pwdInput.addEventListener("keyup", function () {
                checkPasswordStrength(pwdInput.value);
            });
        };
    </script>

    <style>
        .register-container {
            max-width: 450px;
            width: 90%;
            margin: 40px auto;
            background-color: #fdfdfd;
            padding: 30px 25px;
            border-radius: 10px;
            box-shadow: 0 5px 20px rgba(0, 0, 0, 0.1);
            font-family: Arial, sans-serif;
        }

        h2 {
            text-align: center;
            margin-bottom: 25px;
            color: #333;
        }

        label {
            font-weight: bold;
            margin-top: 10px;
            display: block;
        }

        .form-control {
            width: 100%;
            padding: 10px 12px;
            margin-top: 5px;
            border-radius: 5px;
            border: 1px solid #ccc;
            box-sizing: border-box;
            font-size: 14px;
        }

        .btn-submit {
            width: 100%;
            padding: 12px;
            margin-top: 20px;
            background-color: #0078d7;
            color: white;
            font-weight: bold;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .btn-submit:hover {
            background-color: #005ea2;
        }

        .error-message {
            color: red;
            font-size: 13px;
        }

        .strength-bar {
            width: 100%;
            height: 8px;
            background-color: #e0e0e0;
            border-radius: 5px;
            margin-top: 5px;
        }

        .strength-bar div {
            height: 100%;
            border-radius: 5px;
            width: 0%;
        }

        .strength-text {
            margin-top: 5px;
            font-weight: bold;
        }
    </style>
</asp:Content>
