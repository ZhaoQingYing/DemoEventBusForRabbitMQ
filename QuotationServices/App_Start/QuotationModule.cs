using Autofac;
using Autofac.Integration.WebApi;
using DemoEventBusFramewrok;
using DemoEventBusRabbitMQ;
using QuotationServices.EventHandling;
using QuotationServices.Events;
using QuotationServices.Repositories;
using RabbitMQ.Client;
using System.Configuration;
using System.Reflection;
using AutofacModule = Autofac.Module;

namespace QuotationServices.App_Start
{
    public class QuotationModule:AutofacModule
    {
        private readonly string dbConnectString;

        public QuotationModule(string connectString)
        {
            dbConnectString = connectString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<IEventHandler<IEvent>>().As<IEventHandler>();
            builder.RegisterType<QuotationCreateEventHandler>().As<IEventHandler<QuotationCreateEvent>>();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            builder.RegisterInstance(new QuotationRepository(dbConnectString)).As<IQuotationRepository>().SingleInstance();
          
        }

    }
}