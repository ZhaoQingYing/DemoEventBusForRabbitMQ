using Autofac;
using Autofac.Integration.WebApi;
using DemoEventBusFramewrok;
using DemoEventBusRabbitMQ;
using RabbitMQ.Client;
using System.Configuration;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace QuotationServices.App_Start
{
    public class QuotationServicesStarter
    {
        public static void Register(HttpConfiguration config)
        {
            #region JSON格式配置
            config.Formatters.Clear();

            var jsonFormatter = new JsonMediaTypeFormatter()
            {
                Indent = true
            };

            config.Formatters.Add(jsonFormatter);

            #endregion

            var demoBuiler = new ContainerBuilder();

            string sqlConnection = ConfigurationManager.ConnectionStrings["Sqlserver"].ConnectionString;

            demoBuiler.RegisterModule(new QuotationModule(sqlConnection));

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