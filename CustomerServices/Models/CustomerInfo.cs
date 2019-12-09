using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomerServices.Models
{
    public class CustomerInfo
    {
        public Guid Id { get; set; }
        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public int CustomerLevel { get; set; }

        public int CustomerTypeId { get; set; }

        public int IndustryId { get; set; }

        public DateTime CreateDate { get; set; }
    }
}