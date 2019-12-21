using Autofac;
using Autofac.Integration.WebApi;
using CustomerServices.Repositories;
using DemoEventBusFramewrok;
using DemoEventBusRabbitMQ;
using RabbitMQ.Client;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace CustomerServices.App_Start
{
    public class CustomerServicesStarter
    {
        public static void Register(HttpConfiguration config) {

            #region JSON格式配置
            config.Formatters.Clear();

            var jsonFormatter = new JsonMediaTypeFormatter()
            {
                Indent = true
            };

            config.Formatters.Add(jsonFormatter);

            #endregion

            var demoBuiler = new ContainerBuilder();

            demoBuiler.RegisterModule(new CustomerModule());

            //Redis配置

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

            //禁用Redis危险命令
            var safeMap = new HashSet<string>
                {
                    "FLUSHALL","FLUSHDB","CONFIG","KEYS","INFO", "CLUSTER","PING", "ECHO", "CLIENT"
                };

            configRedis.CommandMap = CommandMap.Create(safeMap, false);
            //configRedis.CommandMap = CommandMap.Sentinel;

            demoBuiler.RegisterInstance(new CustomerRepository(configRedis)).As<ICustomerRepository>().SingleInstance();

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
                DispatchConsumersAsync = true
            };

            #endregion

            demoBuiler.RegisterInstance(new DefaultRabbitMQPersistentConnection(factory, retryCount))
               .As<IRabbitMQPersistentConnection>().SingleInstance();

            demoBuiler.RegisterType<InMemoryEventBusSubscriptionsManager>().As<IEventBusSubscriptionsManager>();

            config.DependencyResolver = new AutofacWebApiDependencyResolver(demoBuiler.Build());

            var currentDependencyResolver = config.DependencyResolver as AutofacWebApiDependencyResolver;
            var rabbitMQPersistentConnection = currentDependencyResolver.Container.Resolve<IRabbitMQPersistentConnection>();
            var iLifetimeScope = currentDependencyResolver.Container.Resolve<ILifetimeScope>();
            var eventBusSubcriptionsManager = currentDependencyResolver.Container.Resolve<IEventBusSubscriptionsManager>();
            var clientSubscriptionName = ConfigurationManager.AppSettings["ClientSubscriptionName"];

            var builer = new ContainerBuilder();
            builer.RegisterInstance(new RabbitMQEventBus(rabbitMQPersistentConnection, iLifetimeScope, eventBusSubcriptionsManager, queueName: clientSubscriptionName))
                .As<IEventBus>().SingleInstance();

            builer.Update(currentDependencyResolver.Container as IContainer);
        }
    }
}