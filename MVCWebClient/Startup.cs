using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MVCWebClient.Startup))]
namespace MVCWebClient
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
