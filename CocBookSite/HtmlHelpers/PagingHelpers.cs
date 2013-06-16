using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CocBookSite.Models;
using CocBookSite.ViewModels;
using System.Text.RegularExpressions;

namespace CocBookSite.HtmlHelpers
{
    public static class PagingHelpers
    {
        public static MvcHtmlString PageLinks(this HtmlHelper html, PagingDisplay pagingInfo, Func<int, string> pageUrl)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 1; i <= pagingInfo.TotalPage; i++)
            {
                TagBuilder tag = new TagBuilder("a");
                tag.MergeAttribute("href", pageUrl(i));
                tag.MergeAttribute("style","padding:5px");
                tag.InnerHtml = i.ToString();
                if (i == pagingInfo.CurrentPage)
                {
                    tag.AddCssClass("selected");
                }
                result.Append(tag.ToString());
            }
            return MvcHtmlString.Create(result.ToString());
        }

        public static MvcHtmlString ShortenString(this HtmlHelper html, string Str)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < Str.Length && i<=40; i++)
            {
                result.Append(Str[i]);
            }
            if (Str.Length > 40)
            {
                result.Append("...");
            }
            return MvcHtmlString.Create(result.ToString());
        }

        public static MvcHtmlString ChangeToUrlString(this HtmlHelper html, string str)
        {
            string result;

            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = str.Normalize(NormalizationForm.FormD);
            
            result= regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
            result = result.Replace(" ", "-");
            return MvcHtmlString.Create(result);

        }

        public static MvcHtmlString GetExtendInfo(this HtmlHelper html, string str, int id)
        {
            string[] result;
            char[] token = new char[] { ';' };
            if (str == null || str.Equals(""))
            {
                return MvcHtmlString.Create("");
            }
            result = str.Split(token);
            if (id >= result.Length)
            {
                return MvcHtmlString.Create("");
            }
            else
            {
                return MvcHtmlString.Create(result[id]);
            }

        }


    }
}