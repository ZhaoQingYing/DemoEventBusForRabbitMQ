using Autofac;
using Autofac.Integration.WebApi;
using DemoEventBusFramewrok;
using DemoEventBusRabbitMQ;
using QuotationServices.EventHandling;
using QuotationServices.Events;
using QuotationServices.Repositories;
using RabbitMQ.Client;
using System.Configuration;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;

namespace QuotationServices
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
           
            #region JSON格式配置
            GlobalConfiguration.Configuration.Formatters.Clear();

            var jsonFormatter = new JsonMediaTypeFormatter()
            {
                Indent = true
            };

            GlobalConfiguration.Configuration.Formatters.Add(jsonFormatter);

            #endregion


            //注册所有类型
            var demoBuiler = new ContainerBuilder();
            demoBuiler.RegisterType<IEventHandler<IEvent>>().As<IEventHandler>();
            demoBuiler.RegisterType<QuotationCreateEventHandler>().As<IEventHandler<QuotationCreateEvent>>();
            demoBuiler.RegisterApiControllers(Assembly.GetExecutingAssembly());


            var dbConnectString = ConfigurationManager.ConnectionStrings["Sqlserver"].ConnectionString;
            demoBuiler.RegisterInstance(new QuotationRepository(dbConnectString)).As<IQuotationRepository>().SingleInstance();


            #region RabbitMQ配置
            var host = ConfigurationManager.AppSettings["HostName"];
            var username = ConfigurationManager.AppSettings["UserName"];
            var password = ConfigurationManager.AppSettings["Password"];
            var vhost = ConfigurationManager.AppSettings["VirtualHost"];
            var retryCount = 5;

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["EventBusRetryCount"]))
            {
                retryCount = int.Parse(ConfigurationManager.AppSettings["EventBusRetryCount"]);
            }

            var factory = new ConnectionFactory
            {
                HostName = host,
                UserName = username,
                Password = password,
                VirtualHost = vhost,
                Port = 5672,
                DispatchConsumersAsync=true
            };

            #endregion

            demoBuiler.RegisterInstance(new DefaultRabbitMQPersistentConnection(factory, retryCount))
                .As<IRabbitMQPersistentConnection>().SingleInstance();

            demoBuiler.RegisterType<InMemoryEventBusSubscriptionsManager>().As<IEventBusSubscriptionsManager>();

            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(demoBuiler.Build());

            var currentDependencyResolver = GlobalConfiguration.Configuration.DependencyResolver as AutofacWebApiDependencyResolver;
            var rabbitMQPersistentConnection = currentDependencyResolver.Container.Resolve<IRabbitMQPersistentConnection>();
            var iLifetimeScope = currentDependencyResolver.Container.Resolve<ILifetimeScope>();
            var eventBusSubcriptionsManager = currentDependencyResolver.Container.Resolve<IEventBusSubscriptionsManager>();
            var clientSubscriptionName = ConfigurationManager.AppSettings["ClientSubscriptionName"];

            var builer = new ContainerBuilder();
            builer.RegisterInstance(new RabbitMQEventBus(rabbitMQPersistentConnection, iLifetimeScope, eventBusSubcriptionsManager, queueName: clientSubscriptionName))
                .As<IEventBus>().SingleInstance();

            builer.Update(currentDependencyResolver.Container as IContainer);

            //var eventBus = currentDependencyResolver.Container.Resolve<IEventBus>();
            //eventBus.Subscribe<QuotationCreateEvent, QuotationCreateEventHandler>();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
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
