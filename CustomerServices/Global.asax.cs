using Autofac;
using Autofac.Integration.WebApi;
using CustomerServices.App_Start;
using CustomerServices.Events;
using DemoEventBusFramewrok;
using System.Web.Http;
using System.Web.Mvc;

namespace CustomerServices
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configure(CustomerServicesStarter.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            
        }

        protected void Application_BeginRequest()
        {
            var currentDependencyResolver = GlobalConfiguration.Configuration.DependencyResolver as AutofacWebApiDependencyResolver;
            var eventBus = currentDependencyResolver.Container.Resolve<IEventBus>();
            eventBus.Subscribe<CustomerCreateEvent, IEventHandler<CustomerCreateEvent>>();
        }
    }
}
