using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budget.Models
{
    public class Category
    {
        public Category ()
        {
            this.Transactions = new HashSet<Transaction>();
            this.BudgetItems = new HashSet<BudgetItem>();
        }
        
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public ICollection<Transaction> Transactions { get; set; }
        public ICollection<BudgetItem> BudgetItems { get; set; }
    }
}