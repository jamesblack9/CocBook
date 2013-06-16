using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CocBookSite.Models;

namespace CocBookSite.ViewModels
{
    public class UserProfile
    {
        public Account CusAcc { get; set; }
        public Customer CusInfo { get; set; }
        public List<Order> CusOrders { get; set; }
    }
}