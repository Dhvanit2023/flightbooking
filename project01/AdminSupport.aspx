<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master"
    EnableEventValidation="false" AutoEventWireup="true" CodeBehind="AdminSupport.aspx.cs" Inherits="project01.WebForm26" %>



<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<style>
    .chat-container {
        max-width: 900px;
        margin: 40px auto;
        background: #ffffff;
        border-radius: 10px;
        box-shadow: 0 4px 10px rgba(0,0,0,0.1);
        padding: 20px;
    }
    h2 { text-align:center; color:#007bff; margin-bottom:20px; }
    .chat-box {
        height: 400px;
        overflow-y: auto;
        border: 1px solid #dee2e6;
        border-radius: 8px;
        padding: 15px;
        background: #f8f9fa;
    }
    .message {
        margin: 8px 0;
        padding: 10px 15px;
        border-radius: 15px;
        display: inline-block;
        max-width: 70%;
    }
    .admin { background: #007bff; color: white; float: right; text-align: right; }
    .user { background: #e9ecef; color: black; float: left; }
    .timestamp {
        font-size: 11px;
        color: #6c757d;
        display: block;
        margin-top: 3px;
    }
    .input-section {
        margin-top: 20px;
        display: flex;
        gap: 10px;
    }
    .input-section input {
        flex: 1;
        border-radius: 6px;
        border: 1px solid #ccc;
        padding: 10px;
    }
    .btn-send {
        background: linear-gradient(90deg,#0072ff,#00c6ff);
        border: none;
        color: white;
        padding: 10px 20px;
        border-radius: 8px;
        cursor: pointer;
    }
    .btn-send:hover { background: linear-gradient(90deg,#00c6ff,#0072ff); }
</style>

<div class="chat-container">
    <h2>💬 Admin Support Chat</h2>

    <div id="chatBox" class="chat-box">
        <asp:Repeater ID="rptChat" runat="server">
            <ItemTemplate>
                <div class="message <%# Eval("SenderType").ToString() == "Admin" ? "admin" : "user" %>">
                    <strong><%# Eval("Username") %>:</strong> <%# Eval("MessageText") %>
                    <span class="timestamp"><%# Convert.ToDateTime(Eval("Timestamp")).ToString("dd MMM yyyy, hh:mm tt") %></span>
                </div>
                <div style="clear:both;"></div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <!-- Input Box -->
    <div class="input-section">
        <asp:TextBox ID="txtMessage" runat="server" placeholder="Type your reply..." />
        <asp:Button ID="btnSend" runat="server" Text="Send" CssClass="btn-send" OnClick="btnSend_Click" />
    </div>

    <div class="text-center mt-2">
        <asp:Label ID="lblMsg" runat="server" CssClass="text-success fw-bold"></asp:Label>
    </div>
</div>
<!-- Auto-refresh chat every 5 seconds -->
<script>
    setInterval(function () {
        __doPostBack('<%= btnSend.UniqueID %>', 'refresh');
    }, 5000);
</script>

</asp:Content>

