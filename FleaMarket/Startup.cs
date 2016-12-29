using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FleaMarket.Startup))]
namespace FleaMarket
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
