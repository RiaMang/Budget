using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Budget.HelperExtensions;

namespace Budget.Models
{
    public class Household 
    {

        public Household()
        {
            this.Users = new HashSet<ApplicationUser>();
            this.Accounts = new HashSet<Account>();
            this.BudgetItems = new HashSet<BudgetItem>();
            this.Invitations = new HashSet<Invitation>();
            this.Categories = new HashSet<Category>();
        }
        
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset? RemovedDate { get; set; }
        //public bool IsDeleted { get; set; }
        
        
        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<BudgetItem> BudgetItems { get; set; }
        public virtual ICollection<Invitation> Invitations { get; set; }
        public virtual ICollection<Category> Categories { get; set; }

    }
}