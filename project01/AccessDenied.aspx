<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AccessDenied.aspx.cs" Inherits="project01.AccessDenied" %>


<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Access Denied - FlightBook</title>

    <!-- Bootstrap 5 -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" />

    <!-- Font Awesome -->
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />

    <style>
        body {
            background: linear-gradient(135deg, #0078d7, #002a5c);
            color: #fff;
            font-family: 'Segoe UI', sans-serif;
            height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
        }

        .denied-box {
            background: rgba(255, 255, 255, 0.1);
            backdrop-filter: blur(10px);
            padding: 40px 50px;
            border-radius: 15px;
            text-align: center;
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.3);
        }

        .denied-box i {
            font-size: 70px;
            color: #ff4d4d;
            margin-bottom: 20px;
        }

        .denied-box h2 {
            font-weight: 700;
            margin-bottom: 10px;
        }

        .denied-box p {
            font-size: 16px;
            color: #f1f1f1;
        }

        .btn-custom {
            background-color: #fff;
            color: #0078d7;
            border-radius: 25px;
            padding: 10px 25px;
            font-weight: 600;
            transition: 0.3s;
        }

        .btn-custom:hover {
            background-color: #0078d7;
            color: #fff;
            border: 1px solid #fff;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="denied-box">
            <i class="fa-solid fa-lock"></i>
            <h2>Access Denied</h2>
            <p>You do not have permission to view this page.<br />
               Please log in with an admin account to continue.</p>

            <div class="mt-4">
                <asp:Button ID="btnGoHome" runat="server" Text="🏠 Go to Home"
                    CssClass="btn btn-custom me-2" OnClick="btnGoHome_Click" />
                <asp:Button ID="btnLogin" runat="server" Text="🔐 Login"
                    CssClass="btn btn-custom" OnClick="btnLogin_Click" />
            </div>
        </div>
    </form>
</body>
</html>
