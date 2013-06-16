using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CocBookSite.Models;
namespace CocBookSite.ViewModels
{
    public class Home
    {
        public List<Category> CateList { get; set; }
        public List<V_Book> NewList { get; set; }
        public List<V_Book> HighRatingList { get; set; }
        public List<V_Book> DealList { get; set; }

    }
}