using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CocBookSite.ViewModels;
using CocBookSite.Models;

namespace CocBookSite.Controllers
{
    public class CartController : Controller
    {
        //
        // GET: /Cart/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddToCart(int bookID, int quantity)
        {
            Cart cart = GetCart();
            V_Book b;
            using (var dbContext = new CocBookEntities())
            {
                b = (from c in dbContext.V_Book
                     where c.BookID == bookID
                     select c).SingleOrDefault();

            }
            if (b == null)
            {
                return Json(new { Success = false });
            }
            cart.AddBook(b, quantity);
            return Json(new { Success = true });
        }
    
        [HttpPost]
        public ActionResult RemoveFromCart(int bookID)
        {
            // xoa hang khoi gio
            Cart cart = GetCart();
            // tim hang
            V_Book b;
            using (var dbContext = new CocBookEntities())
            {
                b = (from c in dbContext.V_Book
                     where c.BookID == bookID
                     select c).SingleOrDefault();

            }
            if (b == null)
            {
                return Json(new { Success = false, BName = bookID });
            }
            // co hang thi xoa
            cart.UpdateQuantity(b, 0);
            // tinh lai tong cong
            var sum = cart.GetTotal();

            return Json(new { Success = true ,BName = b.Name, Sum = sum, End = cart.IsEmpty()});
        }

        [HttpPost]
        public ActionResult UpdateCart(int bookID, int quantity)
        {
            // sua hang trong gio
            Cart cart = GetCart();
            // tim hang
            V_Book b;
            using (var dbContext = new CocBookEntities())
            {
                b = (from c in dbContext.V_Book
                     where c.BookID == bookID
                     select c).SingleOrDefault();

            }
            if (b == null)
            {
                return Json(new { Success = false, BName = bookID });
            }
            // co hang thi sua so luong
            cart.UpdateQuantity(b, quantity);

            // tinh lai tong cong
            var sum = cart.GetTotal();
            var tempSum = b.Price * quantity;

            return Json(new { Success = true, Sum = sum, Temp = tempSum });

        }


        public ActionResult GetCartButton()
        {
            return PartialView("CartButton");
        }

        private Cart GetCart()
        {
            Cart cart = (Cart)Session["Cart"];
            if (cart == null)
            {
                cart = new Cart();
                Session["Cart"] = cart;
            }
            return cart;
        }

        public ActionResult Cart()
        {

            return View();
        }
    }
}
