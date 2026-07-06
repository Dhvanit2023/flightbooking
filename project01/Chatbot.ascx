<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Chatbot.ascx.cs" Inherits="project01.Chatbot" %>

<!-- Floating Chat Icon -->
<div id="chatIcon" onclick="toggleChatWindow()">💬
    <span class="tooltip-text">Your Virtual Assist Aero.</span>
</div>

<!-- Hidden Field to store chat open state -->
<asp:HiddenField ID="hfChatOpen" runat="server" Value="false" />

<div id="chatContainer" class="chat-container" style="display:none;">
    <!-- HEADER -->
    <div class="chat-header">
        <span>Chatbot-Aero</span>
        <span id="closeChat" onclick="toggleChatWindow()">❌</span>
    </div>

    <!-- CHAT BODY -->
    <div class="chat-body" id="chatBody">
        <asp:UpdatePanel ID="upChatPanel" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <!-- Chat Box -->
                <asp:Panel ID="pnlChat" runat="server" CssClass="chat-box"></asp:Panel>

                <!-- INPUT AREA -->
                <div class="chat-input">
                    <asp:TextBox ID="txtChatInput" runat="server"
                        CssClass="chat-textbox"
                        placeholder="Type a message..."
                        ClientIDMode="Static" />
                    <asp:Button ID="btnSend" runat="server" Text="Send"
                        CssClass="chat-send-btn"
                        OnClick="btnSend_Click"
                        UseSubmitBehavior="false"
                        ClientIDMode="Static" />
                </div>
            </ContentTemplate>

            <%-- Trigger for partial postback only in chatbot --%>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnSend" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</div>
<script>
    document.addEventListener("DOMContentLoaded", function () {
        const chatInput = document.getElementById("txtChatInput");
        const sendButton = document.getElementById("btnSend");

        if (chatInput && sendButton) {
            chatInput.addEventListener("keydown", function (event) {
                // Detect Enter key
                if (event.key === "Enter" || event.keyCode === 13) {
                    event.preventDefault(); // Prevent newline on mobile keyboards
                    sendButton.click(); // Trigger ASP.NET button click event
                }
            });
        }
    });
</script>

<script type="text/javascript">
    function toggleChatWindow() {
        var chat = document.getElementById("chatContainer");
        var chatIcon = document.getElementById("chatIcon");
        var hf = document.getElementById("<%= hfChatOpen.ClientID %>");


        if (chat.style.display === "none" || chat.style.display === "") {
            // ✅ Show chat, hide icon
            chat.style.display = "flex";
            chatIcon.style.display = "none";
            hf.value = "true";
            focusChatInput();
            scrollToBottom();
        } else {
            // ✅ Close chat, show icon again
            chat.style.display = "none";
            chatIcon.style.display = "block";
            hf.value = "false";
        }
    }

    function scrollToBottom() {
        var chatBody = document.getElementById("chatBody");
        if (chatBody) chatBody.scrollTop = chatBody.scrollHeight;
    }

    function focusChatInput() {
        setTimeout(function () {
            var input = document.getElementById("txtChatInput");
            if (input) input.focus();
        }, 100);
    }

    document.addEventListener('DOMContentLoaded', function () {
        var textbox = document.getElementById('txtChatInput');
        if (textbox) {
            textbox.addEventListener('input', function () {
                this.style.height = 'auto';
                this.style.height = Math.min(this.scrollHeight, 80) + 'px';
            });
        }

        // Restore chat state
        var chat = document.getElementById("chatContainer");
        var chatIcon = document.getElementById("chatIcon");
        var hf = document.getElementById("<%= hfChatOpen.ClientID %>");
        if (hf.value === "true") {
            chat.style.display = "flex";
            chatIcon.style.display = "none";
            scrollToBottom();
            focusChatInput();
        } else {
            chat.style.display = "none";
            chatIcon.style.display = "block";
        }
    });

    document.addEventListener("keydown", function (e) {
        if (e.key === "Enter" && e.target.id === "txtChatInput") {
            e.preventDefault();
            var sendBtn = document.getElementById("btnSend");
            if (sendBtn && !sendBtn.disabled) sendBtn.click();
        }
    });

    // Keep chat open after async postback
    if (typeof Sys !== "undefined") {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            var chat = document.getElementById("chatContainer");
            var chatIcon = document.getElementById("chatIcon");
            var hf = document.getElementById("<%= hfChatOpen.ClientID %>");
            if (hf.value === "true") {
                chat.style.display = "flex";
                chatIcon.style.display = "none";
                scrollToBottom();
                focusChatInput();
            } else {
                chat.style.display = "none";
                chatIcon.style.display = "block";
            }
        });
    }
</script>

<style>
/* CHAT ICON */
#chatIcon {
    position: fixed;
    bottom: 20px;
    right: 20px;
    width: 60px;
    height: 60px;
    background: #007bff;
    color: white;
    font-size: 30px;
    text-align: center;
    line-height: 60px;
    border-radius: 50%;
    cursor: grab;
    z-index: 10000;
    box-shadow: 0 4px 6px rgba(0,0,0,0.2);
    transition: all 0.3s ease;
    user-select: none;
}

#chatIcon:active {
    cursor: grabbing;
}

#chatIcon:hover {
    background: #0056b3;
    transform: scale(1.05);
}

/* Tooltip styling */
.tooltip-text {
    position: absolute;
    bottom: 70px; /* Above the icon */
    left: 50%;
    transform: translateX(-50%) translateY(10px);
    background-color: #333;
    color: #fff;
    padding: 6px 10px;
    border-radius: 5px;
    font-size: 12px;
    white-space: nowrap;
    opacity: 0;
    visibility: hidden;
    transition: opacity 0.3s ease, transform 0.3s ease;
    pointer-events: none;
    z-index: 10001;
}

#chatIcon:hover .tooltip-text {
    opacity: 1;
    visibility: visible;
    transform: translateX(-50%) translateY(0);
}

/* Show tooltip on hover with smooth fade and slide up */
#chatIcon:hover .tooltip-text {
    opacity: 1;
    visibility: visible;
    transform: translateX(-50%) translateY(0);
}
#chatIcon:hover { background: #0056b3; transform: scale(1.05); }

/* CHAT CONTAINER */
.chat-container {
    position: fixed; bottom: 20px; right: 20px;
    width: 350px; height: 500px;
    background: #fff; border: 1px solid #ccc; border-radius: 10px;
    display: none; flex-direction: column; box-shadow: 0 0 15px rgba(0,0,0,0.3);
    z-index: 9999;
}

/* HEADER */
.chat-header {
    display: flex; align-items: center; justify-content: space-between;
    padding: 12px 15px; background: #007bff; color: #fff; font-weight: bold;
    border-radius: 10px 10px 0 0; flex-shrink: 0; position: relative;
}
#closeChat {
    position: absolute; right: 10px; top: 50%; transform: translateY(-50%);
    width: 28px; height: 28px; border-radius: 50%; cursor: pointer;
    display: flex; align-items: center; justify-content: center;
    background: rgba(255,255,255,0.1); border: 1px solid rgba(255,255,255,0.2);
    z-index: 1001; transition: all 0.3s ease;
}
#closeChat:hover { background: rgba(255,255,255,0.3); transform: translateY(-50%) scale(1.1); }

/* CHAT BODY */
.chat-body {
    flex: 1; padding: 10px; overflow-y: auto; background: #f7f7f7;
    display: flex; flex-direction: column; gap: 8px; scroll-behavior: smooth;
}
.chat-body::-webkit-scrollbar { width: 8px; }
.chat-body::-webkit-scrollbar-track { background: #e0e0e0; border-radius: 4px; }
.chat-body::-webkit-scrollbar-thumb { background: #007bff; border-radius: 4px; }
.chat-body::-webkit-scrollbar-thumb:hover { background: #0056b3; }

/* CHAT MESSAGES */
.chat-box { display: flex; flex-direction: column; gap: 8px; }
.msg { padding: 10px 14px; border-radius: 18px; max-width: 75%; font-size: 14px; line-height: 1.4; animation: fadeIn 0.3s ease-in; clear: both; word-wrap: break-word; }
.user { background: #007bff; color: #fff; margin-left: auto; float: right; border-bottom-right-radius: 6px; }
.bot { background: #fff; color: #333; border: 1px solid #e0e0e0; margin-right: auto; float: left; border-bottom-left-radius: 6px; }

/* INPUT AREA FIXED */
.chat-input {
    display: flex; gap: 8px; padding: 12px; border-top: 1px solid #ddd;
    background: #fff; flex-shrink: 0; box-shadow: 0 -2px 4px rgba(0,0,0,0.1);
}
.chat-textbox { flex: 1; padding: 10px 12px; border-radius: 20px; border: 1px solid #ccc; resize: none; max-height: 80px; overflow-y: auto; font-size: 14px; line-height: 1.4; outline: none; }
.chat-textbox:focus { border-color: #007bff; box-shadow: 0 0 0 2px rgba(0,123,255,0.25); }
.chat-send-btn { background: #007bff; color: white; border: none; padding: 10px 16px; border-radius: 20px; min-width: 60px; height: 42px; cursor: pointer; font-weight: 500; transition: all 0.3s ease; }
.chat-send-btn:hover:not(:disabled) { background: #0056b3; transform: translateY(-1px); }
.chat-send-btn:disabled { background: #6c757d; cursor: not-allowed; opacity: 0.7; transform: none; }

/* ANIMATION */
@keyframes fadeIn { from { opacity: 0; transform: translateY(15px); } to { opacity: 1; transform: translateY(0); } }

/* RESPONSIVE */
@media (max-width: 400px) {
    .chat-container { width: 300px; right: 10px; }
    #chatIcon { right: 10px; bottom: 10px; }
}
</style>
