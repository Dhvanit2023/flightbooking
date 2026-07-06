using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(project01.Startup))] // Adjust namespace

namespace project01
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Enable SignalR
            app.MapSignalR();
        }
    }
}
