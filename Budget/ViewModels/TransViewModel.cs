using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budget.Models
{
    public class TransViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Description { get; set; }
        public DateTimeOffset TransDate { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public decimal RecAmount { get; set; }
        public string UpdatedBy { get; set; }
        public int AccountId { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
    }

    public class TrViewModel
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public DateTimeOffset TransDate { get; set;}
        public string Description { get; set; }
        public string Category {get; set;}
        public decimal Amount {get; set;}
        public decimal Reconciled {get; set;}
        public string UpdatedBy {get; set;}
        public string Edit { get; set; }
        public string Delete { get; set; }
    }
}