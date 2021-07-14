using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebOneApp.Startup))]
namespace WebOneApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
