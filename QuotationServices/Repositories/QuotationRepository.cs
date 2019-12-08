using Dapper;
using QuotationServices.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace QuotationServices.Repositories
{
    public class QuotationRepository : IQuotationRepository
    {
        private readonly string connectionString;

        public QuotationRepository(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public async Task AddAsync(Quotation orderEntity)
        {
            var insertSql = @"INSERT INTO [dbo].[QuotationOrder]
                                   ([OrderId]
                                   ,[OfType]
                                   ,[ActualPrice]
                                   ,[OfferPrice]
                                   ,[CreateDate])
                             VALUES
                                   (@OrderId,
                                    @OfType,
                                    @ActualPrice,
                                    @OfferPrice,
                                    @CreateDate)";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                await connection.ExecuteAsync(insertSql, orderEntity);
            }
        }

        public async Task<Quotation> GetAsyn(int Id)
        {
            var querySql = @"SELECT [OrderId]
                                  ,[OfType]
                                  ,[ActualPrice]
                                  ,[OfferPrice]
                                  ,[IsSpeciaApproval]
                                  ,[IsPointsOrder]
                                  ,[CreateDate]
                                  ,[Remark]
                              FROM [dbo].[QuotationOrder] WHERE OrderId=@OrderId";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                return await connection.QueryFirstAsync<Quotation>(querySql, Id);
            }
        }

        public Task<Quotation> UpdateAsync(Quotation orderEntity)
        {
            throw new NotImplementedException();
        }
    }
}