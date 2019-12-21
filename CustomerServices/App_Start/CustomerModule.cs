using Autofac;
using Autofac.Integration.WebApi;
using CustomerServices.EventHandling;
using CustomerServices.Events;
using DemoEventBusFramewrok;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using AutofacModule = Autofac.Module;

namespace CustomerServices.App_Start
{
    public class CustomerModule:AutofacModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<IEventHandler<IEvent>>().As<IEventHandler>();
            builder.RegisterType<CustomerCreateEventHandler>().As<IEventHandler<CustomerCreateEvent>>();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

        }

    }
}