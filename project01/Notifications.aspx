<%@ Page Title="Notifications" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Notifications.aspx.cs" Inherits="project01.Notifications" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>My Notifications</h2>

    <asp:GridView ID="gvNotifications" runat="server" AutoGenerateColumns="False" CssClass="table table-striped" OnRowCommand="gvNotifications_RowCommand">
        <Columns>
            <asp:TemplateField HeaderText="#">
                <ItemTemplate>
                    <span class="notification-number"><%# Container.DataItemIndex + 1 %></span>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Title" HeaderText="Title" />
            <asp:BoundField DataField="Message" HeaderText="Message" />
            <asp:BoundField DataField="CreatedAt" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
            <asp:TemplateField HeaderText="Link">
                <ItemTemplate>
                    <asp:LinkButton ID="lnkOpen" runat="server" Text="Open" CommandName="MarkRead" CommandArgument='<%# Eval("NotificationID") %>'></asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

    <!-- Sound -->
    <audio id="notifSound" src="notification.mp3" preload="auto"></audio>

    <script src="Scripts/jquery-3.6.0.min.js"></script>
    <script src="Scripts/jquery.signalR-2.4.2.min.js"></script>
    <script src="/signalr/hubs"></script>
    <script>
        let soundAllowed = false;
        document.addEventListener('click', function () { soundAllowed = true; }, { once: true });

        $(function () {
            var notifHub = $.connection.notificationHub;

            notifHub.client.receiveNotification = function (notifId, title, message, url) {
                if (soundAllowed) {
                    const audio = document.getElementById('notifSound');
                    audio.currentTime = 0;
                    audio.play().catch(err => console.log("Sound blocked", err));
                }

                var table = $('#<%= gvNotifications.ClientID %> tbody');
                var rowCount = table.find('tr').length + 1;

                var row = '<tr style="background-color:#ffe6e6;" data-id="' + notifId + '">' +
                    '<td class="notification-number" style="color:red;font-weight:bold;">' + rowCount + '</td>' +
                    '<td>' + title + '</td>' +
                    '<td>' + message + '</td>' +
                    '<td>' + new Date().toLocaleString() + '</td>' +
                    '<td><a href="#" onclick="markRead(' + notifId + ', this);return false;">Open</a></td>' +
                    '</tr>';

                table.prepend(row);
            };

            $.connection.hub.start().done(function () {
                console.log("Connected to notification hub!");
            });
        });

        function markRead(notifId, link) {
            $.ajax({
                type: 'POST',
                url: 'MarkNotificationRead.aspx',
                data: { id: notifId },
                success: function () {
                    $(link).closest('tr').remove(); // Remove row after marking read
                },
                error: function () {
                    alert('Error marking notification read');
                }
            });
        }
    </script>

    <style>
        .notification-number { font-weight: bold; color: red; }
        #gvNotifications tr:nth-child(1) { background-color: #ffe6e6; transition: background 1s; }
    </style>
</asp:Content>
