using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CocBookSite.ViewModels;
using CocBookSite.Models;
using CocBookSite.HtmlHelpers;
using System.Text.RegularExpressions;
using System.Text;
using System.Web.Security;
using System.Net.Mail;

namespace CocBookSite.Controllers
{
    public class HomeController : Controller
    {

        private int PageSize = 8;

        #region Inside Helper
        private string ChangeToUrlString(string str)
        {
            string result;

            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = str.Normalize(NormalizationForm.FormD);

            result = regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
            result = result.Replace(" ", "-");
            return (result);
        }

        private string getUser()
        {
            if (Session["username"] != null)
            {
                return Session["username"].ToString();
            }
            else
            {

                HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie == null)
                {
                    return "guest";
                }
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                return ticket.Name == null ? "guest" : ticket.Name;
            }
        }
        #endregion

        #region Book Display
        public ActionResult Index()
        {
            // View model
            Home homeViewModel = new Home();

            // Binding data home page
            using (var DbContext = new CocBookEntities())
            {

                homeViewModel.CateList = (from c in DbContext.Categories
                                          where c.Active == true
                                          orderby c.Position
                                          select c).ToList();
                var books = (from c in DbContext.V_Book
                             where c.Active == true
                             select c).ToList();

                homeViewModel.NewList = (from c in books
                                         orderby c.CreatedDate descending
                                         select c).Take(12).ToList();

                homeViewModel.HighRatingList = (from c in books
                                                orderby c.AveScore descending
                                                select c).Take(12).ToList();

                homeViewModel.DealList = (from c in books
                                          orderby c.DealPercentage descending
                                          select c).Take(12).ToList();
            }
            if (TempData["mess"] != null)
            {
                ViewBag.BuyMess = "xử lý";
            }
            return View(homeViewModel);
        }

        public ActionResult CateFilter(int id, int page = 1)
        {
            bool blnValidCate = false;
            string strCateName = "";
            PagingDisplay pagingView = new PagingDisplay();
            using (var DbContext = new CocBookEntities())
            {
                // list Category
                pagingView.CateList = (from c in DbContext.Categories
                                       where c.Active == true
                                       orderby c.Position
                                       select c).ToList();
                // know Category
                foreach (var item in pagingView.CateList)
                {
                    if (item.CateID == id)
                    {
                        blnValidCate = true;
                        strCateName = item.Name;
                    }
                }

                // if invalid URL
                if (!blnValidCate)
                {
                    return RedirectToAction("Index", "Home");
                }
                // load books
                var books = (from b in DbContext.V_Book
                             join c in DbContext.BookInCategories on b.BookID equals c.BookID
                             where c.CateID == id
                             orderby b.CreatedDate descending
                             select b).ToList();

                pagingView.TotalItem = books.Count;
                pagingView.ItemsPerPage = PageSize;
                pagingView.CurrentPage = page;
                pagingView.TotalPage = (int)Math.Ceiling((decimal)pagingView.TotalItem / pagingView.ItemsPerPage);
                pagingView.BookList = books.Skip((page - 1) * PageSize).Take(PageSize).ToList();
            }
            ViewBag.SelectedID = id;
            ViewBag.Category = strCateName;

            ViewBag.PageCategory = id + "/" + ChangeToUrlString(strCateName);
            return View(pagingView);
        }
        #endregion

        #region Search
        [HttpPost]
        public ActionResult Search(FormCollection form)
        {
            int page = 1;
            string str = form["searchkey"];
            if (str.Trim() == "")
            {
                return RedirectToAction("Index");
            }
            string type = form["searchtype"] == "" ? "1" : form["searchtype"];
            PagingDisplay pagingView = new PagingDisplay();
            using (var DbContext = new CocBookEntities())
            {
                SearchHistory history = new SearchHistory();
                history.SearchValue = str + ";" + type;
                history.Username = getUser();

                pagingView.CateList = (from c in DbContext.Categories
                                       where c.Active == true
                                       orderby c.Position
                                       select c).ToList();
                List<V_Book> books = null;

                if (type.Equals("1"))
                {
                    books = (from c in DbContext.V_Book
                             where c.Active == true && c.Name.Contains(str)
                             orderby c.CreatedDate descending
                             select c).ToList();
                }
                else
                {
                    books = (from c in DbContext.V_Book
                             where c.Active == true && c.AuthorName.Contains(str)
                             orderby c.CreatedDate descending
                             select c).ToList();

                }
                // store history of search
                history.HitCount = books.Count;
                history.CreatedDate = DateTime.Now;
                DbContext.SearchHistories.Add(history);
                DbContext.SaveChanges();

                // data mining search history
                string strS = str + ";" + type;
                var extend = (from c in DbContext.SearchHistories
                              where c.SearchValue.Contains(str) && c.SearchValue.Contains(type) && c.SearchValue != strS && c.HitCount < books.Count
                              orderby c.HitCount descending
                              select c).Take(1).SingleOrDefault();
                int eid = 0;
                string extendSearch = "";
                string extendType = "";
                if (extend != null)
                {
                    eid = extend.AutoID;
                    extendSearch = extend.SearchValue.ToString().Split(';')[0];
                    extendType = extend.SearchValue.ToString().Split(';')[1];
                }


                // display
                pagingView.TotalItem = books.Count;
                pagingView.ItemsPerPage = PageSize;
                pagingView.CurrentPage = page;
                pagingView.TotalPage = (int)Math.Ceiling((decimal)pagingView.TotalItem / pagingView.ItemsPerPage);
                pagingView.BookList = books.Skip((page - 1) * PageSize).Take(PageSize).ToList();
                ViewBag.SearchKey = str;
                ViewBag.SearchType = type;
                ViewBag.ExtendSearch = extendSearch;
                ViewBag.ExtendType = extendType;
                ViewBag.eid = eid;
            }
            return View(pagingView);
        }

        public ActionResult Search(string str, int type = 1, int page = 1)
        {
            PagingDisplay pagingView = new PagingDisplay();
            using (var DbContext = new CocBookEntities())
            {
                pagingView.CateList = (from c in DbContext.Categories
                                       where c.Active == true
                                       orderby c.Position
                                       select c).ToList();
                List<V_Book> books = null;

                if (type == 1)
                {
                    books = (from c in DbContext.V_Book
                             where c.Active == true && c.Name.Contains(str)
                             orderby c.CreatedDate descending
                             select c).ToList();

                }
                else
                {
                    books = (from c in DbContext.V_Book
                             where c.Active == true && c.AuthorName.Contains(str)
                             orderby c.CreatedDate descending
                             select c).ToList();
                }
                pagingView.TotalItem = books.Count;
                pagingView.ItemsPerPage = PageSize;
                pagingView.CurrentPage = page;
                pagingView.TotalPage = (int)Math.Ceiling((decimal)pagingView.TotalItem / pagingView.ItemsPerPage);
                pagingView.BookList = books.Skip((page - 1) * PageSize).Take(PageSize).ToList();
                ViewBag.SearchKey = str;
                ViewBag.SearchType = type + "";
                ViewBag.eid = 0;
            }
            return View(pagingView);
        }
        #endregion

        #region Login
        public ActionResult Login(string username, string password)
        {
            //handle login
            using (var dbContext = new CocBookEntities())
            {
                Account chkAcc = (from c in dbContext.Accounts
                                  where c.Username == username
                                  select c).SingleOrDefault();
                if (chkAcc == null)
                {
                    return Json(new { Success = false, Message = "Tên đăng nhập không đúng" });
                }
                if (chkAcc.Password.Equals(password))
                {
                    if (chkAcc.Active == true)
                    {
                        if (chkAcc.RoleID != 1)
                        {
                            HttpContext.Session.Add("admin", username);
                            return Json(new { Success = true, GoAdmin = true});
                        }
                        else
                        {
                            FormsAuthentication.SetAuthCookie(username, false);
                            HttpContext.Session.Add("username", username);
                            if (Request.UrlReferrer.ToString().Contains("Order/Order"))
                            {
                                return Json(new { Success = true, Reload = true });
                            }
                            else if (Request.UrlReferrer.ToString().Contains("Book/Details"))
                            {
                                return Json(new { Success = true, Reload = true });
                            }
                            else if (Request.UrlReferrer.ToString().Contains("Home/ContactUs"))
                            {
                                return Json(new { Success = true, Reload = true });
                            }
                            return Json(new { Success = true });
                        }
                    }
                    else
                    {
                        return Json(new { Success = false, Message = "Tài khoản đã bị block" });
                    }
                }
                else
                {
                    return Json(new { Success = false, Message = "Mật khẩu không đúng" });
                }
            }
        }

        public ActionResult GetLoginButton()
        {
            return PartialView("LoginButton");
        }

        [HttpPost]
        public ActionResult ForgotPassword(string username, string email)
        {
            //handle login
            using (var dbContext = new CocBookEntities())
            {
                Customer chkCus = (from c in dbContext.Customers
                                   where c.Username == username && c.Email == email
                                   select c).SingleOrDefault();
                if (chkCus == null || email == "")
                {
                    return Json(new { Success = false });
                }
                else
                {
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com", 587);
                    mail.From = new MailAddress("jamsesblack9@gmail.com");
                    mail.To.Add(chkCus.Email);
                    mail.Subject = "Password recovery";
                    mail.Body = "Password : " + chkCus.Account.Password;

                    SmtpServer.EnableSsl = true;
                    SmtpServer.UseDefaultCredentials = false;
                    SmtpServer.Credentials = new System.Net.NetworkCredential("jamesblack9@gmail.com", "P@ssword1992");
                    SmtpServer.Send(mail);

                    return Json(new { Success = true });
                }
            }
        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            cookie1.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie1);

            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        #endregion

        #region Contact

        public ActionResult ContactUs()
        {
            string username = getUser();
            if (username != null && username != "guest")
            {
                using (var dbContext = new CocBookEntities())
                {
                    var cus = (from c in dbContext.Customers
                               where c.Username == username
                               select c).Single();
                    ViewBag.Email = cus.Email;
                }
            }
            return View();
        }

        [HttpPost]
        public ActionResult ContactUs(string usermail,string content)
        {
            using (var dbContext = new CocBookEntities())
            {
                Feedback fb = new Feedback();
                fb.createdate = DateTime.Now;
                fb.email = usermail;
                fb.question = content;
                dbContext.Feedbacks.Add(fb);
                dbContext.SaveChanges();
            }
            return Json(new { Success = true, Message = "Cảm ơn bạn đã góp ý, chúng tôi sẽ trả lời qua email sớm nhất có thể" });
        }
        #endregion

    }
}
