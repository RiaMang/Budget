using Budget.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Budget.HelperExtensions;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using System.Net;
using Newtonsoft.Json;

namespace Budget.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [AllowAnonymous]
        public ActionResult Index()
        {

            //Helper helper = new Helper();
            //var hh = helper.GetHousehold(User.Identity.GetUserId());
            //if(hh == null)
            //{ }
            //var userId = User.Identity.GetUserId();
            //userId.GetHousehold();

            return View();
        }

        [AuthorizeHouseholdRequired]
        public ActionResult Dashboard()
        {
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            
            List<Transaction> model = new List<Transaction>();
            var accounts = db.Accounts.Where(a => a.HouseholdId == hh.Id).ToList();
            var tod = System.DateTimeOffset.Now;
            tod = tod.AddDays(-7);
            foreach (var acc in accounts)
            {
                var tr = acc.Transactions.Where(t => t.TransDate > tod).ToList();
                model.AddRange(tr);
            }
            return View(model);
        }

        public ActionResult GetChart()
        {
            var s = new [] { new { year= "2008", value= 20 },
                new { year= "2009", value= 5 },
                new { year= "2010", value= 7 },
                new { year= "2011", value= 10 },
                new { year= "2012", value= 20 }};

            var house = db.Households.Find(User.Identity.GetHouseholdId<int>());
            var tod = System.DateTimeOffset.Now;
            decimal totalExpense = 0;
            decimal totalBudget = 0;
            var totalAcc = (from a in house.Accounts
                            select a.Balance).DefaultIfEmpty().Sum();
                           

            var bar = (from c in house.Categories
                       where c.CategoryType.Name == "Expense"
                       let aSum = (from t in c.Transactions
                                   where t.TransDate.Year == tod.Year && t.TransDate.Month == tod.Month
                                     select t.Amount).DefaultIfEmpty().Sum()
                       let bSum = (from b in c.BudgetItems
                                       select b.Amount).DefaultIfEmpty().Sum()
                       let _ = totalExpense += aSum
                       let __ = totalBudget += bSum
                       select new
                       {
                           Name = c.Name,
                           Actual = aSum,
                           Budgeted = bSum
                       }).ToArray();
           
            var donut = (from c in house.Categories
                        where c.CategoryType.Name == "Expense"
                        let aSum = (from t in c.Transactions
                                    where t.TransDate.Year==tod.Year && t.TransDate.Month == tod.Month
                                    select t.Amount).DefaultIfEmpty().Sum()
                        select new 
                        {
                            label = c.Name,
                            value = aSum
                        }).ToArray();

            var result = new
            {
                totalAcc = totalAcc,
                totalBudget = totalBudget,
                totalExpense = totalExpense,
                bar = bar,
                donut = donut
            };

            return Content(JsonConvert.SerializeObject(result), "application/json");
        }

        public ActionResult GetMonthly()
        {
            var household = db.Households.Find(User.Identity.GetHouseholdId<int>());
            var monthsToDate = Enumerable.Range(1, DateTime.Today.Month)
                            .Select(m => new DateTime(DateTime.Today.Year, m, 1))
                            .ToList();

            var sums = from month in monthsToDate
                        select new
                        {
                           month = month.ToString("MMM"),
 
                           income = (from account in household.Accounts
                                           from transaction in account.Transactions
                                           where transaction.Category.CategoryType.Name == "Income" && transaction.TransDate.Month == month.Month
                                           select transaction.Amount).DefaultIfEmpty().Sum(),
 
                           expense = (from account in household.Accounts
                                              from transaction in account.Transactions
                                              where transaction.Category.CategoryType.Name == "Expense" && transaction.TransDate.Month == month.Month
                                             select transaction.Amount).DefaultIfEmpty().Sum(),
 
                           budget = household.BudgetItems.Select(b=>b.Amount).DefaultIfEmpty().Sum(),
                                             //budget = household.BudgetItems.Select(b => b.Amount * (b.Frequency / 12)).DefaultIfEmpty().Sum()
                        };
                        
              //var barData = new {
              //    income = sums.ToDictionary(k=> k.month, v=>v.income),
              //      expense = sums.ToDictionary(k=>k.month, v=>v.expense),
              //      budget = sums.ToDictionary(k=>k.month, v=>v.budget)                    
              //          };


              return Content(JsonConvert.SerializeObject(sums), "application/json");
        }

        [AuthorizeHouseholdRequired]
        public PartialViewResult _RecentTransactions()
        {
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            List<Transaction> model = new List<Transaction>();
            var accounts = db.Accounts.Where(a => a.HouseholdId == hh.Id).ToList();
            var tod = System.DateTimeOffset.Now;
            tod=tod.AddDays(-7);
            foreach (var acc in accounts)
            {
                var tr = acc.Transactions.Where(t => t.TransDate > tod).ToList();
                model.AddRange(tr);
            }
            return PartialView(model);
        }

        [AuthorizeHouseholdRequired]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [AuthorizeHouseholdRequired]
        public ActionResult Household()
        {
            //var userid = User.Identity.GetUserId();
            
            return View(db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId())));
        }

        [Authorize]
        public ActionResult CreateJoinHousehold()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateJoinHousehold(HouseholdViewModel hvm)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            if (hvm.Code == null) // create household
            {
                if (hvm.Name == null)
                    return View();
                Household h = new Household { Name = hvm.Name };
                db.Households.Add(h);
                db.SaveChanges();
                h.AddCategories();
                
                user.HouseholdId = h.Id;
            }
            else // join household
            {
                
                    var invite = db.Invitations.FirstOrDefault(m => m.Code == hvm.Code);
                    if(invite != null && user.Email == invite.Email)
                    {
                        user.HouseholdId = invite.HouseholdId;
                        db.Entry(user).Property(p => p.HouseholdId).IsModified = true;
                    }
                    else
                    {
                        ViewBag.Error = "Sorry, The code and email combination does not match. ";
                        return View();
                    }
                
            }
            //ApplicationSignInManager SignInManager = HttpContext.GetOwinContext().Get<ApplicationSignInManager>();

            //HttpContext.GetOwinContext().Authentication.SignOut();
            //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            await ControllerContext.HttpContext.RefreshAuthentication(user);
            db.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        [AuthorizeHouseholdRequired]
        public ActionResult InviteToHousehold()
        {
            return View();
        }

        [AuthorizeHouseholdRequired]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InviteToHousehold(Invitation invite)
        {
            var userid = User.Identity.GetUserId();
            var user = db.Users.Find(userid);
            
            invite.InviteUserToHousehold(user);

            return RedirectToAction("Dashboard");
        }



        [AuthorizeHouseholdRequired]
        public async Task<ActionResult> LeaveHousehold()
        {
            var userid = User.Identity.GetUserId();
            var user = db.Users.Find(userid);
            db.Users.RemoveUserFromHousehold(user.Id);
            await ControllerContext.HttpContext.RefreshAuthentication(user);

            //var user1 = new ApplicationUser() { Id = userid, UserName = User.Identity.GetUserName() }; // just trying if this works, could use 'user' too.
            //ApplicationSignInManager SignInManager = HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            
            //HttpContext.GetOwinContext().Authentication.SignOut();
            //await SignInManager.SignInAsync(user1, isPersistent: false, rememberBrowser: false);

            return RedirectToAction("Household");
        }
    }
}