using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using CocBookSite.Models;
using System.Net.Mail;


namespace CocBookSite.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            if (Session["admin"] == null)
            {
                ViewBag.Mess = "Please login as admin";
                return View("Error");
            }
            return View();
        }

        #region Book
        public ActionResult ManageBook()
        {
            return View();
        }

        public ActionResult GVAddBook()
        {
            List<Category> lcate;
            using (var dbContext = new CocBookEntities())
            {
                lcate = (from c in dbContext.Categories
                         where c.Active == true
                         select c).ToList();
            }
            return PartialView("AddBook", lcate);
        }

        [HttpPost]
        public ActionResult AddBook(FormCollection form)
        {
            string bookname = form["bookname"];
            string authorname = form["authorname"];
            string publisher = form["publisher"];
            int No = Convert.ToInt32("0" + form["No"]);
            string ptime = form["ptime"];

            double rprice = Convert.ToDouble("0" + form["rprice"]);
            double iprice = Convert.ToDouble("0" + form["iprice"]);
            double sprice = Convert.ToDouble("0" + form["sprice"]);

            string info = form["info"];
            string extendinfo = form["extendinfo"];

            int cate = Convert.ToInt32(form["cate"]);

            var dbContext = new CocBookEntities();

            Book b = new Book();
            b.Name = bookname;
            b.AuthorName = authorname;
            b.Info = info;
            b.ExtendInfo = extendinfo;
            b.Publisher = publisher;
            b.NoOfPublish = No;
            b.PublishTime = ptime;
            b.BuyPrice = iprice;
            b.ShowPrice = rprice;
            b.CreatedDate = DateTime.Now;
            b.Active = true;

            dbContext.Books.Add(b);
            dbContext.SaveChanges();

            // file img
            HttpPostedFileBase upfile = Request.Files["pic"];
            string filename = b.BookID + ".jpg";
            upfile.SaveAs(Server.MapPath(Path.Combine("~/Content/img-book", filename)));

            b.Avatar = "img-book/" + filename;
            dbContext.SaveChanges();

            // price
            BookPrice bp = new BookPrice();
            bp.BookID = b.BookID;
            bp.Price = sprice;
            bp.ApplyTime = DateTime.Now;
            dbContext.BookPrices.Add(bp);
            dbContext.SaveChanges();

            // category
            BookInCategory bic = new BookInCategory();
            bic.CateID = cate;
            bic.BookID = b.BookID;
            dbContext.BookInCategories.Add(bic);
            dbContext.SaveChanges();

            return RedirectToAction("ManageBook", "Admin");
        }

        public ActionResult ListBook()
        {
            List<Book> lbook;
            using (var dbContext = new CocBookEntities())
            {
                lbook = (from c in dbContext.Books
                         orderby c.CreatedDate descending
                         select c).ToList();
            }
            return PartialView("ListBook", lbook);
        }

        public ActionResult EditBook(int bookid)
        {
            V_Book b;
            using (var dbContext = new CocBookEntities())
            {
                b = (from c in dbContext.V_Book
                     where c.BookID == bookid
                     select c).Single();
            }
            return PartialView("EditBook", b);
        }

        [HttpPost]
        public ActionResult SaveEditBook(FormCollection form)
        {
            int bookID = Convert.ToInt32(form["bid"]);
            string active = form["chkActive"];
            string bookname = form["bookname"];
            string authorname = form["authorname"];
            string publisher = form["publisher"];
            int No = Convert.ToInt32("0" + form["No"]);
            string ptime = form["ptime"];

            double rprice = Convert.ToDouble("0" + form["rprice"]);
            double iprice = Convert.ToDouble("0" + form["iprice"]);
            double sprice = Convert.ToDouble("0" + form["sprice"]);

            string info = form["info"];
            string extendinfo = form["extendinfo"];

            int cate = Convert.ToInt32(form["cate"]);

            var dbContext = new CocBookEntities();

            var b = (from c in dbContext.Books where c.BookID == bookID select c).Single();
            b.Name = bookname;
            b.AuthorName = authorname;
            b.Info = info;
            b.ExtendInfo = extendinfo;
            b.Publisher = publisher;
            b.NoOfPublish = No;
            b.PublishTime = ptime;
            b.BuyPrice = iprice;
            b.ShowPrice = rprice;
            b.CreatedDate = DateTime.Now;
            b.Active = (active == null) ? false : true;

            dbContext.SaveChanges();

            // file img
            HttpPostedFileBase upfile = Request.Files["pic"];
            if (upfile.ContentLength > 0)
            {
                string filename = b.BookID + ".jpg";
                upfile.SaveAs(Server.MapPath(Path.Combine("~/Content/img-book", filename)));
                b.Avatar = "img-book/" + filename;
                dbContext.SaveChanges();
            }

            // price
            BookPrice bp = new BookPrice();
            bp.BookID = b.BookID;
            bp.Price = sprice;
            bp.ApplyTime = DateTime.Now;
            dbContext.BookPrices.Add(bp);
            dbContext.SaveChanges();

            // category
            BookInCategory bic = (from c in dbContext.BookInCategories where c.BookID == b.BookID select c).SingleOrDefault();
            if (bic == null)
            {
                bic = new BookInCategory();
                bic.CateID = cate;
                bic.BookID = b.BookID;
                dbContext.BookInCategories.Add(bic);
            }
            else
            {
                bic.CateID = cate;
                bic.BookID = b.BookID;
            }
            dbContext.SaveChanges();

            return RedirectToAction("ManageBook", "Admin");
        }
        #endregion

        #region Category
        public ActionResult ManageCategory()
        {
            return View();
        }

        public ActionResult GVAddCategory()
        {
            return PartialView("AddCategory");
        }

        [HttpPost]
        public ActionResult AddCategory(FormCollection form)
        {
            string catename = form["bookname"];

            var dbContext = new CocBookEntities();

            Category c = new Category();
            c.Name = catename;
            c.Active = true;

            dbContext.Categories.Add(c);
            dbContext.SaveChanges();

            return RedirectToAction("ManageCategory", "Admin");
        }

        public ActionResult ListCategory()
        {
            List<Category> lcate;
            using (var dbContext = new CocBookEntities())
            {
                lcate = (from c in dbContext.Categories
                         select c).ToList();
            }
            return PartialView("ListCategory", lcate);
        }

        public ActionResult EditCate(int cateid)
        {
            Category b;
            using (var dbContext = new CocBookEntities())
            {
                b = (from c in dbContext.Categories
                     where c.CateID == cateid
                     select c).Single();
            }
            return PartialView("EditCate", b);
        }

        public ActionResult SaveEditCate(FormCollection form)
        {
            int cateID = Convert.ToInt32(form["cid"]);
            string active = form["chkActive"];
            string catename = form["catename"];

            var dbContext = new CocBookEntities();

            var b = (from c in dbContext.Categories where c.CateID == cateID select c).Single();
            b.Name = catename;
            b.Active = (active == null) ? false : true;

            dbContext.SaveChanges();

            return RedirectToAction("ManageCategory", "Admin");
        }
        #endregion

        #region Order
        public ActionResult ManageOrder()
        {
            return View();
        }

        public ActionResult ListOrder()
        {
            List<Order> lorder;
            using (var dbContext = new CocBookEntities())
            {
                lorder = (from c in dbContext.Orders
                          orderby c.RequestDate descending
                          select c).ToList();
            }
            return PartialView("ListOrder", lorder);
        }

        public ActionResult Delivery(int id)
        {
            // process
            using (var dbContext = new CocBookEntities())
            {
                var order = (from c in dbContext.Orders where c.OrderID == id select c).Single();
                order.Status = "Delivery";
                dbContext.SaveChanges();
            }
            return RedirectToAction("ManageOrder", "Admin");
        }

        public ActionResult Cancel(int id)
        {
            // process
            using (var dbContext = new CocBookEntities())
            {
                var order = (from c in dbContext.Orders where c.OrderID == id select c).Single();
                order.Status = "Cancel";
                dbContext.SaveChanges();
            }
            return RedirectToAction("ManageOrder", "Admin");
        }
        #endregion

        #region Feedback
        public ActionResult ManageFeedback()
        {
            return View();
        }

        public ActionResult ListFeedback()
        {
            List<Feedback> lfeedback;
            using (var dbContext = new CocBookEntities())
            {
                lfeedback = (from c in dbContext.Feedbacks
                             orderby c.createdate descending
                             select c).ToList();
            }
            return PartialView("ListFeedback", lfeedback);
        }

        public ActionResult Answer(int fid)
        {
            Feedback b;
            using (var dbContext = new CocBookEntities())
            {
                b = (from c in dbContext.Feedbacks
                     where c.AutoID == fid
                     select c).Single();
            }
            return PartialView("Answer", b);
        }
        public ActionResult Spam(int fid, string answer)
        {
            Feedback b;
            using (var dbContext = new CocBookEntities())
            {
                b = (from c in dbContext.Feedbacks
                     where c.AutoID == fid
                     select c).Single();
                b.answer = answer;
                b.valid = false;
                dbContext.SaveChanges();
            }
            return Json(new { Success = true });

        }
        public ActionResult AnswerFeedback(int fid, string answer)
        {
            Feedback b;
            using (var dbContext = new CocBookEntities())
            {
                b = (from c in dbContext.Feedbacks
                     where c.AutoID == fid
                     select c).Single();
                b.answer = answer;
                b.answerdate = DateTime.Now;


                try
                {
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com", 587);
                    mail.From = new MailAddress("jamsesblack9@gmail.com");
                    mail.To.Add(b.email);
                    mail.Subject = "Feedback";
                    mail.Body = "Question : " + b.question + "\n.Answer :" + b.answer;
                    SmtpServer.EnableSsl = true;
                    SmtpServer.UseDefaultCredentials = false;
                    SmtpServer.Credentials = new System.Net.NetworkCredential("jamesblack9@gmail.com", "P@ssword1992");
                    SmtpServer.Send(mail);
                }
                catch (Exception e)
                {
                    b.answer = e.Message;
                    b.valid = false;
                }
                dbContext.SaveChanges();
            }
            return Json(new { Success = true });
        }
        public ActionResult ViewFeedback(int fid)
        {
            Feedback b;
            using (var dbContext = new CocBookEntities())
            {
                b = (from c in dbContext.Feedbacks where c.AutoID == fid select c).Single();
            }
            return PartialView("ViewFeedback", b);
        }
        #endregion
    }
}
