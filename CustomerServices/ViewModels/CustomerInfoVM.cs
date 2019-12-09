using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomerServices.ViewModels
{
    public class CustomerInfoVM
    {
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>

        public string CustomerName { get; set; }

        /// <summary>
        /// 客户类型编号
        /// </summary>
        public int CustomerTypeId { get; set; }

        /// <summary>
        /// 客户等级
        /// </summary>
        public int CustomerLevel { get; set; }

        /// <summary>
        /// 所属行业编号
        /// </summary>
        public int IndustryId { get; set; }

        /// <summary>
        /// 创建时间戳
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 客户联系信息
        /// </summary>
        //public ICollection<ContactInfoVM> ContactInfo { get;set; }

    }
}