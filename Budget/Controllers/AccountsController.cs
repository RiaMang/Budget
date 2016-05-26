using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace Budget.Models
{   
    [AuthorizeHouseholdRequired]
    public class AccountsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Accounts
        public ActionResult Index()
        {
            //var user = db.Users.Find(User.Identity.GetUserId());
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            var accounts = hh.Accounts.AsQueryable().Include(a => a.Household);
            
            return View(accounts.ToList());
        }

        // GET: Accounts/Details/5
        [Route("Accounts/{id?}/Transactions")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            Account account = db.Accounts.Find(id);
            if (!hh.Accounts.Contains(account)) // if account id does not belong to household - refuse access
                account = null;
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        // GET: Accounts/Create
        public PartialViewResult _Create()
        {
            //ViewBag.Household = new SelectList(db.Households, "Id", "Name");
            return PartialView();
        }

        // POST: Accounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Balance")] Account account)
        {
            if (ModelState.IsValid)
            {
                account.HouseholdId = Convert.ToInt32(User.Identity.GetHouseholdId());
                db.Accounts.Add(account);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", account.HouseholdId);
            return View(account);
        }

        // GET: Accounts/Edit/5
        public PartialViewResult _Edit(int? id)
        {
            
            Account account = db.Accounts.Find(id);
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            if (!hh.Accounts.Contains(account)) // if account id does not belong to household - refuse access
                account = null;
            
            //ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", account.HouseholdId);
            return PartialView(account);
        }

        // POST: Accounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Balance,HouseholdId")] Account account)
        {
            if (ModelState.IsValid)
            {
                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", account.HouseholdId);
            return View(account);
        }

        // GET: Accounts/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Account account = db.Accounts.Find(id);
        //    if (account == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(account);
        //}

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Account account = db.Accounts.Find(id);
            db.Accounts.Remove(account);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
