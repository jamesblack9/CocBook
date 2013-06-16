using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CocBookSite.Models;
using CocBookSite.ViewModels;
namespace CocBookSite.Controllers
{
    public class CategoryController : Controller
    {
        //
        // GET: /Category/
        private int PageSize = 8;
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NewBooks(int page=1)
        {
            PagingDisplay pagingView = new PagingDisplay();
            using (var DbContext = new CocBookEntities())
            {
                pagingView.CateList = (from c in DbContext.Categories
                                          where c.Active == true
                                          orderby c.Position
                                          select c).ToList();
                var books = (from c in DbContext.V_Book
                             where c.Active == true
                             orderby c.CreatedDate descending
                             select c).ToList();

                pagingView.TotalItem = books.Count;
                pagingView.ItemsPerPage = PageSize;
                pagingView.CurrentPage = page;
                pagingView.TotalPage = (int)Math.Ceiling((decimal) pagingView.TotalItem/ pagingView.ItemsPerPage);
                pagingView.BookList = books.Skip((page - 1) * PageSize).Take(PageSize).ToList();
            }
            return View(pagingView);
        }

        public ActionResult BestBooks(int page=1)
        {
            PagingDisplay pagingView = new PagingDisplay();
            using (var DbContext = new CocBookEntities())
            {
                pagingView.CateList = (from c in DbContext.Categories
                                       where c.Active == true
                                       orderby c.Position
                                       select c).ToList();
                var books = (from c in DbContext.V_Book
                             where c.Active == true
                             orderby c.AveScore descending
                             select c).ToList();

                pagingView.TotalItem = books.Count;
                pagingView.ItemsPerPage = PageSize;
                pagingView.CurrentPage = page;
                pagingView.TotalPage = (int)Math.Ceiling((decimal)pagingView.TotalItem / pagingView.ItemsPerPage);
                pagingView.BookList = books.Skip((page - 1) * PageSize).Take(PageSize).ToList();
            }
            return View(pagingView);
        }
        public ActionResult DealBooks(int page=1)
        {
            PagingDisplay pagingView = new PagingDisplay();
            using (var DbContext = new CocBookEntities())
            {
                pagingView.CateList = (from c in DbContext.Categories
                                       where c.Active == true
                                       orderby c.Position
                                       select c).ToList();
                var books = (from c in DbContext.V_Book
                             where c.Active == true
                             orderby c.DealPercentage descending
                             select c).ToList();

                pagingView.TotalItem = books.Count;
                pagingView.ItemsPerPage = PageSize;
                pagingView.CurrentPage = page;
                pagingView.TotalPage = (int)Math.Ceiling((decimal)pagingView.TotalItem / pagingView.ItemsPerPage);
                pagingView.BookList = books.Skip((page - 1) * PageSize).Take(PageSize).ToList();
            }
            return View(pagingView);
        }
    }
}
