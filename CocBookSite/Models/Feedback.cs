//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CocBookSite.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Feedback
    {
        public int AutoID { get; set; }
        public string email { get; set; }
        public string question { get; set; }
        public string answer { get; set; }
        public Nullable<bool> valid { get; set; }
        public Nullable<System.DateTime> createdate { get; set; }
        public Nullable<System.DateTime> answerdate { get; set; }
        public string admin { get; set; }
    
        public virtual Account Account { get; set; }
    }
}