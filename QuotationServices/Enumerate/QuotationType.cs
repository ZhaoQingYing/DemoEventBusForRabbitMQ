using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuotationServices.Enumerate
{
    /// <summary>
    /// 报价单类型
    /// </summary>
    public enum QuotationType
    {
        /// <summary>
        /// 买货单
        /// </summary>
        Sale,

        /// <summary>
        /// 工程单
        /// </summary>
        Project,

        /// <summary>
        /// 借货单
        /// </summary>
        Borrow
    }
}