using Budget.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.ViewModels
{
    public class AccountsViewModel
    {
        public AccountsViewModel()
        {
            this.Transactions = acc.Transactions;
        }
        
        public Account acc { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }
}