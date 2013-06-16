using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CocBookSite.Models;

namespace CocBookSite.ViewModels
{
    public class Payment
    {
        public Customer Cus { get; set; }
        public Cart Cart { get; set; }
    }
}