using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Budget.Models;
using Newtonsoft.Json;

namespace Budget.Controllers
{
    [AuthorizeHouseholdRequired]
    public class BudgetItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BudgetItems
        public ActionResult Index()
        {
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            var budgetItems = hh.BudgetItems.AsQueryable().Include(b => b.Category).Include(b => b.Household);
            return View(budgetItems.ToList());
        }

        // GET: BudgetItems/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BudgetItem budgetItem = db.BudgetItems.Find(id);
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            if (!hh.BudgetItems.Contains(budgetItem)) // if BudgetItem id does not belong to household - refuse access
                budgetItem = null;
            if (budgetItem == null)
            {
                return HttpNotFound();
            }
            return View(budgetItem);
        }

        public ActionResult GetChart()
        {
            var hh = db.Households.Find(User.Identity.GetHouseholdId<int>());
            
            var donut = (from c in hh.Categories
                         where c.CategoryType.Name=="Expense"
                        let sum = (from b in c.BudgetItems
                                   select b.Amount).DefaultIfEmpty().Sum()
                        select new
                        {
                            label = c.Name,
                            value = sum,

                        }).ToArray();
            return Content(JsonConvert.SerializeObject(donut), "application/json");
        }

        // GET: BudgetItems/Create
        public PartialViewResult _Create()
        {
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            ViewBag.CategoryId = new SelectList(hh.Categories, "Id", "Name");
            return PartialView();
        }

        // POST: BudgetItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Amount,CategoryId")] BudgetItem budgetItem)
        {
            if (ModelState.IsValid)
            {
                budgetItem.HouseholdId = Convert.ToInt32(User.Identity.GetHouseholdId());
                db.BudgetItems.Add(budgetItem);
                db.SaveChanges();
                return RedirectToAction("Index","BudgetItems");
            }
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            ViewBag.CategoryId = new SelectList(hh.Categories, "Id", "Name", budgetItem.CategoryId);
            //ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", budgetItem.HouseholdId);
            return RedirectToAction("Index", "BudgetItems", new { budgetItem = budgetItem });
        }

        // GET: BudgetItems/Edit/5
        public PartialViewResult Edit(int? id)
        {
            if (id == null)
            {
                return PartialView(null);
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            BudgetItem budgetItem = db.BudgetItems.Find(id);
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            if (!hh.BudgetItems.Contains(budgetItem)) // if budgetItem id does not belong to household - refuse access
                budgetItem = null;
            if (budgetItem == null)
            {
                return PartialView(null);
                // return HttpNotFound();
            }
           
            ViewBag.CategoryId = new SelectList(hh.Categories, "Id", "Name", budgetItem.CategoryId);
            //ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", budgetItem.HouseholdId);
            return PartialView(budgetItem);
        }

        // POST: BudgetItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Amount,CategoryId,HouseholdId")] BudgetItem budgetItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(budgetItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            ViewBag.CategoryId = new SelectList(hh.Categories, "Id", "Name", budgetItem.CategoryId);
           // ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", budgetItem.HouseholdId);
            return View(budgetItem);
        }

        // GET: BudgetItems/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    BudgetItem budgetItem = db.BudgetItems.Find(id);
        //    var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
        //    if (!hh.BudgetItems.Contains(budgetItem)) // if budgetItem id does not belong to household - refuse access
        //        budgetItem = null;
        //    if (budgetItem == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(budgetItem);
        //}

        // POST: BudgetItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BudgetItem budgetItem = db.BudgetItems.Find(id);
            db.BudgetItems.Remove(budgetItem);
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
