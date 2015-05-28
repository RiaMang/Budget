using Budget.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Budget.HelperExtensions;

namespace Budget.Controllers
{
    [AuthorizeHouseholdRequired]
    public class BudgetsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        
        // GET: Budget
        public ActionResult Index()
        {
            //BudgetViewModel bvm = new BudgetViewModel();
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            ICollection<BudgetMod> Buds = hh.GetBudget();
            return View(Buds);
        }
    }
}