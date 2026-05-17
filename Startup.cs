using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FoodCo.Startup))]
namespace FoodCo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
