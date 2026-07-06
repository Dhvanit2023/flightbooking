<%@ Page Title="Community Support Chat" Language="C#" MasterPageFile="~/Site1.Master"
    AutoEventWireup="true" EnableEventValidation="false"
    CodeBehind="CommunitySupport.aspx.cs" Inherits="project01.CommunitySupport" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        body {
            background-color: #f4f6f9;
        }
        .chat-wrapper {
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 30px 15px;
            min-height: 80vh;
        }
        .chat-container {
            width: 100%;
            max-width: 750px;
            background: #ffffff;
            border-radius: 10px;
            padding: 20px;
            box-shadow: 0 4px 15px rgba(0,0,0,0.1);
        }
        h3 {
            text-align: center;
            color: #007bff;
            font-weight: bold;
            margin-bottom: 20px;
        }
        .chat-box {
            height: 400px;
            overflow-y: auto;
            background: #f8f9fa;
            border: 1px solid #ccc;
            padding: 12px;
            margin-bottom: 15px;
            border-radius: 8px;
        }
        .msg {
            margin: 6px 0;
            padding: 8px 12px;
            border-radius: 10px;
            max-width: 75%;
            display: inline-block;
            word-wrap: break-word;
        }
        .msg-user {
            background: #007bff;
            color: #fff;
            float: right;
            text-align: right;
            clear: both;
        }
        .msg-admin {
            background: #e8f5e9;
            color: #2e7d32;
            float: left;
            text-align: left;
            clear: both;
        }
        .username {
            font-weight: bold;
            font-size: 0.9rem;
        }
        .timestamp {
            font-size: 0.75rem;
            color: #555;
            display: block;
        }
        .input-area {
            display: flex;
            gap: 10px;
        }
        .input-area input {
            flex: 1;
            border: 1px solid #ccc;
            border-radius: 6px;
            padding: 10px;
        }
        .btn-send {
            background: linear-gradient(90deg, #0072ff, #00c6ff);
            border: none;
            color: white;
            padding: 10px 20px;
            border-radius: 8px;
            cursor: pointer;
            transition: 0.3s;
        }
        .btn-send:hover {
            background: linear-gradient(90deg, #00c6ff, #0072ff);
            transform: scale(1.05);
        }
    </style>

    <div class="chat-wrapper">
        <div class="chat-container">
            <h3>🌾 Community Support Chat</h3>
            <asp:Label ID="lblStatus" runat="server" ForeColor="Green"></asp:Label>

            <asp:UpdatePanel ID="UpdatePanelChat" runat="server">
                <ContentTemplate>
                    <div id="chatBox" runat="server" class="chat-box"></div>

                    <div class="input-area">
                        <asp:TextBox ID="txtMessage" runat="server" placeholder="Type your message..." CssClass="form-control" TextMode="SingleLine"></asp:TextBox>
                        <asp:Button ID="btnSend" runat="server" Text="Send" CssClass="btn-send" OnClick="btnSend_Click" />
                    </div>

                    <asp:Timer ID="Timer1" runat="server" Interval="3000" OnTick="Timer1_Tick"></asp:Timer>
                </ContentTemplate>

                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnSend" EventName="Click" />
                    <asp:AsyncPostBackTrigger ControlID="Timer1" EventName="Tick" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </div>
    
    <script type="text/javascript">
        function ScrollChatToBottom() {
            var chatDiv = document.getElementById('<%= chatBox.ClientID %>');
            if (chatDiv) {
                chatDiv.scrollTop = chatDiv.scrollHeight;
            }
        }

        // Run the function on initial page load
        window.onload = ScrollChatToBottom;
        
        // Run the function after every partial postback (AJAX UpdatePanel update)
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(ScrollChatToBottom);
        }
    </script>
</asp:Content>