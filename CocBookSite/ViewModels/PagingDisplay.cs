using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CocBookSite.Models;

namespace CocBookSite.ViewModels
{
    public class PagingDisplay
    {
        public List<Category> CateList { get; set; }
        public List<V_Book> BookList { get; set; }
        public int TotalItem { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPage{ get; set; }
    }
}