using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DemoEventBusFramewrok
{
    public interface IEventHandler
    {
        bool CanHandle(IEvent @event);
    }

    public interface IEventHandler<T> : IEventHandler where T : IEvent {
        Task<bool> HandleAsync(T @event);
    }
}
