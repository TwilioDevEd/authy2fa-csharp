using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebApplication19.Startup))]
namespace WebApplication19
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
