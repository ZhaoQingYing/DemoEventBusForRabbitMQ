using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using QuotationServices.Models;

namespace QuotationServices.Repositories
{
    public interface IQuotationRepository
    {
        Task<Quotation> GetAsyn(int Id);

        Task AddAsync(Quotation orderEntity);

        Task<Quotation> UpdateAsync(Quotation orderEntity);
    }
}