using Ninject;
using Ninject.Web.Common.OwinHost;
using Ninject.Web.WebApi.OwinHost;
using Owin;
using System.Web.Http;

namespace ExtractionService
{
    namespace Configs
    {
        public class StartupConfig
        {
            // add an extra parameter of type IKernel
            public void Configure(IAppBuilder appBuilder, IKernel kernel)
            {
                var config = new HttpConfiguration();

                config.MapHttpAttributeRoutes();
                config.MapDefinedRoutes();

                // plug the passed-in kernel into the Ninject middleware
                appBuilder.UseNinjectMiddleware(() => kernel);

                // start injecting objects into WepApi controllers
                appBuilder.UseNinjectWebApi(config);
            }
        }
    }
}
