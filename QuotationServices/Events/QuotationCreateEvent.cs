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
        public QuotationCreateEvent(Guid id, decimal price, decimal initialPrice, QuotationType orderType)
        {
            Id = id;
            Price = price;
            InitialPrice = initialPrice;
            OrderType = orderType;
            Timestamp = DateTime.Now;
        }

        public QuotationType OrderType { get; }

        public Guid Id { get; }

        public decimal Price { get; }

        public decimal InitialPrice { get; }

        public DateTime Timestamp { get; }
    }
}