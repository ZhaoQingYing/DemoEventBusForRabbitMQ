﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QuotationServices.Enumerate;

namespace QuotationServices.ViewModels
{
    public class QuotationOrderVM
    {
        public QuotationType OrderType { get; set; }

        public decimal Price { get; set; }

        public decimal InitialPrice { get; set; }

        public CustomerInfoVM Customer { get; set; }
    }
}