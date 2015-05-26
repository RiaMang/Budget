using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.Models
{
    public class BudgetViewModel
    {
        public ICollection<BudgetItem> BudgetItems { get; set; }
        public decimal biIncTotal { get; set; }
        public decimal biExpTotal { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
        public decimal trIncTotal { get; set; }
        public decimal trExpTotal { get; set; }
        public decimal Net { get; set; }
    }

    public class BudgetMod
    {
        public string Category { get; set; }
        public string Type { get; set; }
        public decimal EstAmount { get; set; }
        public decimal ActAmout { get; set; }

    }
}