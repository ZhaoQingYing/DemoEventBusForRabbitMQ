using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using DemoEventBusFramewrok;
using QuotationServices.Events;
using QuotationServices.Repositories;
using QuotationServices.Models;

namespace QuotationServices.EventHandling
{
    public class QuotationCreateEventHandler : IEventHandler<QuotationCreateEvent>
    {

        private readonly IQuotationRepository _quotationRepository;
        public QuotationCreateEventHandler(IQuotationRepository quotationRepository)
        {
            _quotationRepository = quotationRepository;
        }


        public bool CanHandle(IEvent @event)
        {
            return @event.GetType().Equals(typeof(QuotationCreateEvent));
        }

        private Task<bool> ExecutionAsync(QuotationCreateEvent @event)
        {
            Quotation newOrder = new Quotation
            {
                OrderId = @event.Id,
                OfferPrice = @event.Price,
                ActualPrice = @event.RealPrice,
                TypeOf = @event.AsType,
                CreateDate = @event.Timestamp
            };

            _quotationRepository.AddAsync(newOrder);

            return Task.FromResult(true);
        }

        public Task<bool> HandleAsync(QuotationCreateEvent @event)
        {
            return CanHandle(@event) ? ExecutionAsync(@event) : Task.FromResult(false);
        }
    }
}