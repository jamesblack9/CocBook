using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CocBookSite.ViewModels;
using CocBookSite.Models;
using System.Web.Security;

namespace CocBookSite.Controllers
{
    public class BookController : Controller
    {
        //
        // GET: /Book/

        #region viewdetail
        public ActionResult Index()
        {
            return View();
        }

        // GET: /Book/Detail/id/name
        public ActionResult Details(int id)
        {
            using (var dbContext = new CocBookEntities())
            {
                var book = (from c in dbContext.V_Book
                            where c.Active == true && c.BookID == id
                            select c).SingleOrDefault();
                if (book == null)
                {
                    ViewBag.Message = "Đã có lỗi xảy ra trong quá trình xử lý thông tin. Xin bạn thử lại sau";
                    return View("Error.cshtml");
                }
                ViewBag.Title = book.Name;
                string username = getUser();
                bool blnRate = true;
                string strRateMess = "Hãy cho điểm sách";
                if (username.Equals("guest"))
                {
                    blnRate = false;
                    strRateMess = "Đăng nhập để cho điểm sách";
                }
                else
                {

                    var rated = (from c in dbContext.Ratings
                                 where c.BookID == id && c.Username == username
                                 select c).SingleOrDefault();
                    if (rated != null)
                    {
                        blnRate = false;
                        strRateMess = "Bạn đã chấm " + rated.Score + " điểm";
                    }
                }
                ViewBag.Rate = blnRate;
                ViewBag.RateMess = strRateMess;
                return View(book);
            }

        }
        #endregion

        #region Rating
        public ActionResult Rating(int bookID, double score)
        {
            string username = getUser();
            if (username == null)
            {
                return Json(new { Success = false, Message = "Bạn chưa đăng nhập" });
            }
            using (var dbContext = new CocBookEntities())
            {
                var rating = new Rating();
                rating.RateDate = DateTime.Now;
                rating.BookID = bookID;
                rating.Username = username;
                rating.Score = score;
                dbContext.Ratings.Add(rating);
                dbContext.SaveChanges(); 
            }

            return Json(new { Success = true , Message="Bạn đã chấm " +score+ " điểm"});
        }
        #endregion

        #region helper
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
    }
}
