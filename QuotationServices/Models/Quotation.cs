using QuotationServices.Enumerate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuotationServices.Models
{
    [Serializable]
    public class Quotation
    {
        public Guid OrderId { get; set; }

        public QuotationType TypeOf { get; set; }

        /// <summary>
        /// 实际金额
        /// </summary>
        public decimal ActualPrice { get; set; }

        /// <summary>
        /// 报价金额
        /// </summary>
        public decimal OfferPrice { get; set; }

        /// <summary>
        /// 是否一键特批
        /// </summary>
        public bool IsSpeciaApproval { get; set; }

        /// <summary>
        /// 是否是积分单
        /// </summary>
        public bool IsPointsOrder { get; set; }

        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 报价单备注
        /// </summary>
        public string Remark { get; set; }
        
    }
}