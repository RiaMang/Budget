using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budget.Models
{
    public class HouseholdViewModel
    {
        [Display(Name="Household Name")]
        public string Name { get; set; }

        [Display(Name = "Invitation Code")]
        public string Code { get; set; }
    }

    public class TransByCatViewModel
    {
        public ICollection<Category> Categories { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }

    public class TransByTypeViewModel
    {
        public ICollection<CategoryType> Types { get; set; }
        public ICollection<Category> Categories { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }
}