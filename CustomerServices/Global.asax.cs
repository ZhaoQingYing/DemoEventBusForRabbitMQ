using Autofac;
using Autofac.Integration.WebApi;
using DemoEventBusFramewrok;
using DemoEventBusRabbitMQ;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using CustomerServices.Repositories;
using StackExchange.Redis;
using CustomerServices.EventHandling;
using CustomerServices.Events;

namespace CustomerServices
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            #region JSON∏Ò Ω≈‰÷√
            GlobalConfiguration.Configuration.Formatters.Clear();

            var jsonFormatter = new JsonMediaTypeFormatter()
            {
                Indent = true
            };

            GlobalConfiguration.Configuration.Formatters.Add(jsonFormatter);

            #endregion

            var demoBuiler = new ContainerBuilder();
            demoBuiler.RegisterType<IEventHandler<IEvent>>().As<IEventHandler>();
            demoBuiler.RegisterType<CustomerCreateEventHandler>().As<IEventHandler<CustomerCreateEvent>>();
            demoBuiler.RegisterApiControllers(Assembly.GetExecutingAssembly());

            //Redis≈‰÷√

            var configRedis = new ConfigurationOptions();
            //configRedis.ServiceName = "";
            configRedis.EndPoints.Add("192.168.197.129", 6379);
            configRedis.EndPoints.Add("192.168.197.128", 6379);
            configRedis.EndPoints.Add("192.168.197.130", 6379);
            configRedis.Password = "123456";

            configRedis.ResolveDns = true;

            //https://stackexchange.github.io/StackExchange.Redis/Configuration#tiebreakers-and-configuration-change-announcements
            configRedis.ConfigurationChannel = "";
            configRedis.TieBreaker = "";//

            //Ω˚”√RedisŒ£œ’√¸¡Ó
            var safeMap = new HashSet<string>
                {
                    "FLUSHALL","FLUSHDB","CONFIG","KEYS","INFO", "CLUSTER","PING", "ECHO", "CLIENT"
                };

            configRedis.CommandMap = CommandMap.Create(safeMap, false);
            //configRedis.CommandMap = CommandMap.Sentinel;

            demoBuiler.RegisterInstance(new CustomerRepository(configRedis)).As<ICustomerRepository>().SingleInstance();


            #region RabbitMQ≈‰÷√
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
                DispatchConsumersAsync = true
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

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
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
