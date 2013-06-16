using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CocBookSite.Models;

namespace CocBookSite.ViewModels
{
    public class Invoice
    {
        public Order Order { get; set; }
        public List<OrderDetail> OrderLine { get; set; }
    }
}