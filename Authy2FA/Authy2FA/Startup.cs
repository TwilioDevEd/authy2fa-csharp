using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Authy2FA.Startup))]
namespace Authy2FA
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
