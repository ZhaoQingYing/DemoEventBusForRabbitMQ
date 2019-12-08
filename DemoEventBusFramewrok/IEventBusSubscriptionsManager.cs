using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoEventBusFramewrok
{
    public interface IEventBusSubscriptionsManager
    {
        bool IsEmpty { get; }

        event EventHandler<string> OnEventRemoved;

        void AddSubscription<TEvent, TEventHandler>()
           where TEvent : IEvent
           where TEventHandler : IEventHandler<TEvent>;

        void RemoveSubscription<TEvent, TEventHandler>()
             where TEventHandler : IEventHandler<TEvent>
             where TEvent : IEvent;
 
        bool HasSubscriptionsForEvent<TEvent>() where TEvent : IEvent;
        bool HasSubscriptionsForEvent(string eventName);
        Type GetEventTypeByName(string eventName);
        void Clear();
        IEnumerable<Type> GetHandlersForEvent<TEvent>() where TEvent : IEvent;
        IEnumerable<Type> GetHandlersForEvent(string eventName);
        string GetEventKey<TEvent>();
    }
}
