using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoEventBusFramewrok
{
    public interface IEventSubscriber
    {
        void Subscribe<TEvent,TEventHandler>() where TEvent:IEvent where TEventHandler:IEventHandler<TEvent>;
    }
}
