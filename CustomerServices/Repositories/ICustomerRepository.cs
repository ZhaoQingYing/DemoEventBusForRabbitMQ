using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomerServices.Models;

namespace CustomerServices.Repositories
{
    public interface ICustomerRepository
    {
        Task<List<CustomerInfo>> GetCustomersAsync();

        Task<CustomerInfo> GetCustomerAsync(string id);

        Task AddCustomerAsync(CustomerInfo customerEntity);

    }
}
