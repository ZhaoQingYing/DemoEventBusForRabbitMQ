using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CustomerServices.Repositories;
using CustomerServices.ViewModels;
using CustomerServices.Models;

namespace CustomerServices.Controllers
{
    public class ValuesController : ApiController
    {
        private readonly ICustomerRepository _customerRepository;

        public ValuesController(ICustomerRepository customerRepository) {
            _customerRepository = customerRepository;
        }

        // GET api/values
        public async Task<IEnumerable<CustomerInfoVM>> Get()
        {
            var customerList = new List<CustomerInfoVM>();
            var customers= await _customerRepository.GetCustomersAsync();
            if (customers!=null&&customers.Count>0) {

                foreach (var customer in customers) {
                    customerList.Add(MapTo(customer));  
                }
            }

            return customerList;
        }

        private CustomerInfoVM MapTo(CustomerInfo info) {

            if (info != null)
            {
                var customerVm = new CustomerInfoVM
                {
                    CustomerName = info.CustomerName,
                    CustomerCode = info.CustomerCode,
                    IndustryId = info.IndustryId,
                    CustomerLevel = info.CustomerLevel,
                    CustomerTypeId = info.CustomerTypeId,
                    CreateDate = info.CreateDate,
                };

                return customerVm;
            }

            return null;
        }

        // GET api/values/5
        public async Task<CustomerInfoVM> Get(int id)
        {
            var custmoer=await _customerRepository.GetCustomerAsync(id.ToString());
            return MapTo(custmoer);
        }

        // POST api/values
        public void Post([FromBody]string value)
        {

        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
