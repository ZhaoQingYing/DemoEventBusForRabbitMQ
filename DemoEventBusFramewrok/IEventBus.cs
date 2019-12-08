using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoEventBusFramewrok
{
    public interface IEventBus:IEventPublisher,IEventSubscriber
    {
       
    }
}
