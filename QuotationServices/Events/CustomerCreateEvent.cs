using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DemoEventBusFramewrok;

namespace QuotationServices.Events
{
    public class CustomerCreateEvent:IEvent
    {
        public CustomerCreateEvent(Guid id, string customerName, string customerCode, int customerLevel)
        {
            Id = id;
            CustomerName = customerName;
            CustomerCode = customerCode;
            CustomerLevel = customerLevel;
            Timestamp = DateTime.UtcNow;
        }

        public int CustomerLevel { get;}

        public string CustomerName { get; }

        public string CustomerCode { get; }


        public Guid Id { get; }

        public DateTime Timestamp { get; }
    }
}