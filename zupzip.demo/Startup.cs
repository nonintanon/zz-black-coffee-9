using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(zupzip.demo.Startup))]
namespace zupzip.demo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
