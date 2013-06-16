using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CocBookSite.Models;
using System.Web.Security;
using CocBookSite.ViewModels;
using System.Net.Mail;

namespace CocBookSite.Controllers
{
    public class CustomerController : Controller
    {
        public string RandomCode()
        {
            Random random = new Random();
            string r = "";
            int i;
            for (i = 0; i < 5; i++)
            {
                r += random.Next(0, 9).ToString();
            }
            return r;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        public ActionResult Active(string activeString)
        {
            string code = activeString.Substring(0, 5);
            string username = activeString.Substring(5);

            using (var dbContext = new CocBookEntities())
            {
                var ticket = (from c in dbContext.ActivationTickets
                              where c.Username == username && c.ActiveCode == code
                              select c).SingleOrDefault();
                if (ticket == null)
                {
                    ViewBag.Mess = "Kích hoạt không thành công";
                    ViewBag.Title = "Lỗi";
                    return View("Error");
                }
                else
                {
                    if (ticket.ActiveDate == null)
                    {
                        //active user
                        var acc = (from b in dbContext.Accounts
                                   where b.Username == username
                                   select b).Single();
                        acc.Active = true;
                        ticket.ActiveDate = DateTime.Now;

                        dbContext.SaveChanges();
                        ViewBag.Mess = "Tài khoản đã được kích hoạt thành công";
                        ViewBag.Title = "Kích hoạt tài khoản";
                        return View("Error");
                    }
                    else
                    {
                        ViewBag.Mess = "Mã kích hoạt đã từng được sử dụng, xin thử lại với mã kích hoạt mới";
                        ViewBag.Title = "Lỗi";
                        return View("Error");
                    }
                }
            }
            return View();
        }

        [HttpPost]
        public ActionResult Register(FormCollection form)
        {
            string username = form["user-name"];
            string password = form["pass-word"];
            string fullname = form["full-name"];
            string email = form["user-email"];
            string phone = form["phone"];
            string address = form["address"];
            string district = form["district"];

            Account newAcc = new Account();
            newAcc.Active = false;
            newAcc.Username = username;
            newAcc.Password = password;
            newAcc.RoleID = 1;

            Customer newCus = new Customer();
            newCus.Username = username;
            newCus.Fullname = fullname;
            newCus.Phone = phone;
            newCus.Email = email;
            newCus.District = district;
            newCus.Street = address;
            newCus.City = "HCM";
            newCus.Point = 0;

            // kich hoat mail
            ActivationTicket ticket = new ActivationTicket();
            ticket.Username = username;
            ticket.CreatedDate = DateTime.Now;
            ticket.ActiveCode = RandomCode();

            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com", 587);
            mail.From = new MailAddress("jamsesblack9@gmail.com");
            mail.To.Add(email);
            mail.Subject = "Account Activation";
            mail.Body = "Here is the link to active your account : http://localhost:9841/Customer/Active/?activeString=" + ticket.ActiveCode + username + "";

            SmtpServer.EnableSsl = true;
            SmtpServer.UseDefaultCredentials = false;
            SmtpServer.Credentials = new System.Net.NetworkCredential("jamesblack9@gmail.com", "P@ssword1992");
            SmtpServer.Send(mail);

            using (var dbContext = new CocBookEntities())
            {
                dbContext.Accounts.Add(newAcc);
                dbContext.Customers.Add(newCus);
                dbContext.ActivationTickets.Add(ticket);
                dbContext.SaveChanges();
            }

            FormsAuthentication.SetAuthCookie(username, false);
            HttpContext.Session.Add("username", username);

            return RedirectToAction("Profile");

        }

        public ActionResult Profile()
        {
            using (var dbContext = new CocBookEntities())
            {
                string username = (string)HttpContext.Session["username"];
                if (username == null)
                {
                    HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                    if (authCookie != null)
                    {
                        FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                        username = ticket.Name;
                        HttpContext.Session.Add("username", username);
                    }
                }
                Customer cus = (from c in dbContext.Customers
                                where c.Username == username
                                select c).SingleOrDefault();
                if (cus == null)
                {
                    //return View("Error");
                    cus = new Customer();
                }
                UserProfile ups = new UserProfile();
                ups.CusInfo = cus;
                if (TempData["InfoMess"] != null)
                {
                    ViewBag.InfoMess = TempData["InfoMess"];
                }

                //retrive order
                var orders = (from c in dbContext.Orders
                              where c.Username == username
                              orderby c.RequestDate descending
                              select c).ToList();
                ups.CusOrders = orders;

                return View(ups);

            }

        }

        [HttpPost]
        public ActionResult UpdateInfo(FormCollection form)
        {
            string fullname = form["full-name"];
            string email = form["user-email"];
            string phone = form["phone"];
            string address = form["address"];
            string district = form["district"];

            if (HttpContext.Session["username"] == null)
            {
                return View("Error");
            }
            using (var dbContext = new CocBookEntities())
            {
                string username = (string)HttpContext.Session["username"];

                Customer cus = (from c in dbContext.Customers
                                where c.Username == username
                                select c).Single();
                cus.Fullname = fullname;
                cus.Phone = phone;
                cus.Email = email;
                cus.District = district;
                cus.Street = address;
                cus.City = "HCM";
                dbContext.SaveChanges();

                UserProfile ups = new UserProfile();
                ups.CusInfo = cus;
                TempData["InfoMess"] = "Thông tin tài khoản đã được cập nhật...";

                return RedirectToAction("Profile", "Customer");

            }

        }
    }
}
