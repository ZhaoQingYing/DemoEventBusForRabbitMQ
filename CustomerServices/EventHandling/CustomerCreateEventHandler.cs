using CustomerServices.Events;
using CustomerServices.Models;
using CustomerServices.Repositories;
using DemoEventBusFramewrok;
using System.Threading.Tasks;

namespace CustomerServices.EventHandling
{
    public class CustomerCreateEventHandler: IEventHandler<CustomerCreateEvent>
    {

        private readonly ICustomerRepository _customerRepository;

        public CustomerCreateEventHandler(ICustomerRepository customerRepository) {
            _customerRepository = customerRepository;
        }

        private Task<bool> ExecutionAsync(CustomerCreateEvent @event)
        { 
            var customer = new CustomerInfo() {
                Id = @event.Id,
                CustomerCode = @event.CustomerCode,
                CustomerName = @event.CustomerName,
                CreateDate=@event.Timestamp
            };

            _customerRepository.AddCustomerAsync(customer);

            return Task.FromResult(true);
        }

        public bool CanHandle(IEvent @event)
        {
            return @event.GetType() == typeof(CustomerCreateEvent);
        }

        public Task<bool> HandleAsync(CustomerCreateEvent @event)
        {
            return CanHandle(@event) ? ExecutionAsync(@event) : Task.FromResult(false);
        }
    }
}