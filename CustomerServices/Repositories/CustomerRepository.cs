using CustomerServices.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace CustomerServices.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string cacheKey = nameof(CustomerInfo);

        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public CustomerRepository(ConfigurationOptions redisOptions)
        {
            _redis = ConnectionMultiplexer.Connect(redisOptions);
            _database = _redis.GetDatabase();
        }

        public async Task AddCustomerAsync(CustomerInfo customerEntity)
        {
            await _database.HashSetAsync(cacheKey, customerEntity.Id.ToString(), JsonConvert.SerializeObject(customerEntity),flags:CommandFlags.DemandMaster);
        }

        public async Task<CustomerInfo> GetCustomerAsync(string id)
        {
            var data = await _database.HashGetAsync(cacheKey, id,flags:CommandFlags.DemandSlave);
            if (data.IsNullOrEmpty)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<CustomerInfo>(data);
        }

        public async Task<List<CustomerInfo>> GetCustomersAsync()
        {
            List<CustomerInfo> customerInfos = new List<CustomerInfo>();
            var hashValues = await _database.HashValuesAsync(cacheKey,flags:CommandFlags.DemandSlave);
            foreach (var value in hashValues)
            {
                customerInfos.Add(JsonConvert.DeserializeObject<CustomerInfo>(value));
            }

            return customerInfos;
        }
    }
}