using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budget.Models
{
    public class BudgetItem
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public decimal Amount { get; set; }
        [Display(Name="Category")]
        public int? CategoryId { get; set; }
        public int HouseholdId { get; set; }

        public virtual Category Category { get; set; }
        public virtual Household Household { get; set; }
    }
}