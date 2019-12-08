using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuotationServices.Models
{
    public class QuotationItem
    {
        private string _productName;
        private decimal _unitPrice;
        public int ProductId { get; private set; }

        public QuotationItem(int productId, string productName, decimal unitPrice)
        {
            ProductId = productId;

            _productName = productName;
            _unitPrice = unitPrice;
        }
    }
}