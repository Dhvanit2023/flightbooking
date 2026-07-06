using Microsoft.AspNet.SignalR;

namespace project01
{
    public class NotificationHub : Hub
    {
        public void SendNotification(string title, string message, string url)
        {
            Clients.All.receiveNotification(title, message, url);
        }
    }
}
