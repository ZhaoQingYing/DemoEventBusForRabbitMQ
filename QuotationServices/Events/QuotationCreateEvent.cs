using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DemoEventBusFramewrok;
using QuotationServices.Enumerate;

namespace QuotationServices.Events
{
    public class QuotationCreateEvent : IEvent
    {
        public QuotationCreateEvent(Guid id, decimal price, decimal realPrice, QuotationType toType)
        {
            Id = id;
            Price = price;
            RealPrice = realPrice;
            AsType = toType;
            Timestamp = DateTime.Now;
        }

        public QuotationType AsType { get; }

        public Guid Id { get; }

        public decimal Price { get; }

        public decimal RealPrice { get; }

        public DateTime Timestamp { get; }
    }
}