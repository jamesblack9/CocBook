using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CocBookSite.Models
{
    public class Cart
    {
        public List<CartLine> lineCollection = new List<CartLine>();

        public void AddBook(V_Book b, int quantity)
        {
            CartLine line = lineCollection.Where(p => p.Book.BookID == b.BookID).FirstOrDefault();
            if (line == null)
            {
                lineCollection.Add(new CartLine(b, quantity));
            }
            else
            {
                line.Quantity += quantity;
            }

        }

        public void UpdateQuantity(V_Book b, int quantity)
        {
            CartLine line = lineCollection.Where(p => p.Book.BookID == b.BookID).FirstOrDefault();
            if (line == null)
            {
            }
            else
            {
                line.Quantity = quantity;
            }
            if (line.Quantity == 0)
            {
                lineCollection.Remove(line);
            }
        }

        public void Clear()
        {
            lineCollection.Clear();
        }

        public bool IsEmpty()
        {
            return lineCollection.Count == 0;
        }

        public double GetTotal()
        {
            double sum = 0;
            for (int i = 0; i < lineCollection.Count; i++)
            {
                sum += lineCollection[i].Quantity * (double)lineCollection[i].Book.Price;
            }
            return sum;
        }
        public class CartLine
        {
            public V_Book Book { get; set; }
            public int Quantity { get; set; }

            public CartLine()
            {

            }
            public CartLine(V_Book b, int quantity)
            {
                this.Book = b;
                this.Quantity = quantity;
            }
        }
    }
}