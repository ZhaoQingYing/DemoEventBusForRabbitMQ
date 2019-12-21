using Autofac;
using Autofac.Integration.WebApi;
using DemoEventBusFramewrok;
using QuotationServices.App_Start;
using QuotationServices.Events;
using System.Web.Http;
using System.Web.Mvc;

namespace QuotationServices
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configure(QuotationServicesStarter.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
           
        }

        protected void Application_BeginRequest()
        {
            var currentDependencyResolver = GlobalConfiguration.Configuration.DependencyResolver as AutofacWebApiDependencyResolver;
            var eventBus = currentDependencyResolver.Container.Resolve<IEventBus>();
            eventBus.Subscribe<QuotationCreateEvent, IEventHandler<QuotationCreateEvent>>();
        }
    }
}
