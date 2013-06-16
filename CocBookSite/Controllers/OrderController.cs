using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CocBookSite.Models;
using CocBookSite.ViewModels;

namespace CocBookSite.Controllers
{
    public class OrderController : Controller
    {
        //
        // GET: /Order/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Order()
        {
            Cart cart = (Cart)Session["Cart"];
            if (cart == null || cart.lineCollection.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            string username = (string)Session["username"];
            Customer cus;

            if (username == null)
            {
                HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie != null)
                {
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                    username = ticket.Name;
                }
            }
            if (username == null)
            {
                cus = new Customer();
            }
            else
            {
                using (var dbContext = new CocBookEntities())
                {
                    cus = (from c in dbContext.Customers
                           where c.Username == username
                           select c).Single();
                }
            }

            Payment payment = new Payment();
            payment.Cus = cus;
            payment.Cart = cart;
            return View(payment);
        }

        [HttpPost]
        public ActionResult Order(FormCollection form)
        {
            #region GetInfo
            Cart cart = (Cart)Session["Cart"];
            if (cart == null || cart.IsEmpty())
            {
                return RedirectToAction("Index", "Home");
            }
            string username = getUser();
            Customer cus;

            if (username == null)
            {
                cus = new Customer();
                cus.Username = "guest";
            }
            else
            {
                using (var dbContext = new CocBookEntities())
                {
                    cus = (from c in dbContext.Customers
                           where c.Username == username
                           select c).Single();
                }
            }
            string fullname = form["full-name"];
            string phone = form["phone"];
            string district = form["district"];
            string address = form["address"];
            string payMethod = form["optPaymentMethod"];
            string speed = form["optSpeed"];
            string comment = form["comment"];
            #endregion
            int oid;
            #region process order
            using (var dbContext = new CocBookEntities())
            {
                Order order = new Order();
                order.Username = cus.Username;
                order.Status = "Created";
                order.RequestDate = DateTime.Now;
                order.Notes = comment;
                order.Total = cart.GetTotal();
                order.Fullname = fullname;
                order.Phone = phone;
                order.District = district;
                order.Street = address;
                order.City = "HCM";
                order.Elog = "";
                order.GiftCode = "";
                // luu order
                dbContext.Orders.Add(order);
                dbContext.SaveChanges();
                for (int i = 0; i < cart.lineCollection.Count; i++)
                {
                    OrderDetail od = new OrderDetail();
                    od.BookID = cart.lineCollection[i].Book.BookID;
                    od.OrderID = order.OrderID;
                    od.Quantity = cart.lineCollection[i].Quantity;
                    od.Price = cart.lineCollection[i].Book.Price;
                    dbContext.OrderDetails.Add(od);
                }
                dbContext.SaveChanges();
                oid = order.OrderID;
            }

            #endregion
            // xoa gio hang
            cart.Clear();
            Session["cart"] = cart;

            TempData["mess"] = "Xử lý";

            return RedirectToAction("Invoice", "Order", new { id = oid });
        }

        public ActionResult Invoice(int id)
        {

            Invoice invoice = new Invoice();
            using (var dbContext = new CocBookEntities())
            {
                var order = (from c in dbContext.Orders
                             where c.OrderID == id
                             select c).Single();
                var orderlines = (from c in dbContext.OrderDetails.Include("Book")
                                  where c.OrderID == id
                                  select c).ToList();
                if (Session["Admin"] != null)
                {
                   
                        invoice.Order = order;
                        invoice.OrderLine = orderlines;
                        return View(invoice);
                }
                else if (order.Username != getUser())
                {
                    if (order.Username == "guest")
                    {
                        TempData["mess"] = "xử lý";
                    }
                    return RedirectToAction("Index", "Home");
                }
                invoice.Order = order;
                invoice.OrderLine = orderlines;
            }
            return View(invoice);
        }


        public ActionResult ReEdit(int id)
        {
            string username = getUser();
            if (Session["Admin"] != null)
            {

            }
            else if (username == null || username == "guest")
            {
                return RedirectToAction("Index", "Home");
            }
            using (var dbContext = new CocBookEntities())
            {

                var order = (from c in dbContext.Orders
                             where c.OrderID == id
                             select c).Single();
                if (Session["Admin"] == null)
                {
                    if (order.Username != username)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                if (order.Status == "Created")
                {
                    var lines = (from c in dbContext.OrderDetails.Include("Book")
                                 where c.OrderID == id
                                 select c).ToList();
                    Invoice inc = new Invoice();
                    inc.Order = order;
                    inc.OrderLine = lines;
                    ViewBag.HiddenID = order.OrderID;
                    return View(inc);
                }
                else
                {
                    return RedirectToAction("Invoice", "Order", new { id = order.OrderID });
                }
            }
            return View();
        }

        [HttpPost]
        public ActionResult Update(FormCollection form)
        {
            string fullname = form["full-name"];
            string phone = form["phone"];
            string district = form["district"];
            string address = form["address"];
            string payMethod = form["optPaymentMethod"];
            string speed = form["optSpeed"];
            string comment = form["comment"];
            int oid = Convert.ToInt32(form["oid"]);

            using (var dbContext = new CocBookEntities())
            {
                var order = (from c in dbContext.Orders
                             where c.OrderID == oid
                             select c).SingleOrDefault();
                if (order == null)
                {
                    return RedirectToAction("Profile", "Customer");
                }
                else
                {
                    order.Status = "Created";
                    order.RequestDate = DateTime.Now;
                    order.Notes = comment;
                    order.Fullname = fullname;
                    order.Phone = phone;
                    order.District = district;
                    order.Street = address;
                    order.City = "HCM";
                    dbContext.SaveChanges();
                }
            }

            return RedirectToAction("Invoice", "Order", new { id = oid });
        }

        [HttpPost]
        public ActionResult UpdateOrder(int bookID, int quantity, int oid)
        {
            //validate

            // edit order detail
            double sum = 0;
            double tempSum = 0;
            using (var dbContext = new CocBookEntities())
            {
                var detail = (from c in dbContext.OrderDetails
                              where c.OrderID == oid && c.BookID == bookID
                              select c).Single();
                detail.Quantity = quantity;
                tempSum = (double)detail.Quantity * (double)detail.Price;
                dbContext.SaveChanges();
                // tinh lai total
                var orderlines = (from c in dbContext.OrderDetails
                                  where c.OrderID == oid
                                  select c).ToList();
                for (int i = 0; i < orderlines.Count; i++)
                {
                    sum += (double)orderlines[i].Quantity * (double)orderlines[i].Price;
                }

                var order = (from c in dbContext.Orders
                             where c.OrderID == oid
                             select c).Single();
                order.Total = sum;
                dbContext.SaveChanges();

            }
            return Json(new { Success = true, Sum = sum, Temp = tempSum });
        }

        [HttpPost]
        public ActionResult RemoveItem(int bookID, int oid)
        {
            double sum = 0;
            using (var dbContext = new CocBookEntities())
            {
                var detail = (from c in dbContext.OrderDetails
                              where c.OrderID == oid && c.BookID == bookID
                              select c).Single();
                dbContext.OrderDetails.Remove(detail);
                dbContext.SaveChanges();
                // tinh lai total
                var orderlines = (from c in dbContext.OrderDetails
                                  where c.OrderID == oid
                                  select c).ToList();
                for (int i = 0; i < orderlines.Count; i++)
                {
                    sum += (double)orderlines[i].Quantity * (double)orderlines[i].Price;
                }

                var order = (from c in dbContext.Orders
                             where c.OrderID == oid
                             select c).Single();
                order.Total = sum;
                dbContext.SaveChanges();
                if (sum == 0)
                {
                    dbContext.Orders.Remove(order);
                }
                dbContext.SaveChanges();
            }
            return Json(new { Success = true, Sum = sum });
        }

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
