using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Budget.Models;

namespace Budget.Controllers
{
    [AuthorizeHouseholdRequired]
    public class CategoriesController : Controller
    {
        
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Categories
        public ActionResult Index()
        {
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            var categories = hh.Categories.AsQueryable().Include(c => c.CategoryType);
            return View(categories.ToList());
        }

        // GET: Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            if (!hh.Categories.Contains(category)) // if category id does not belong to household - refuse access
                category = null;
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Categories/Create
        public PartialViewResult _Create()
        {
            ViewBag.CategoryTypeId = new SelectList(db.CategoryTypes, "Id", "Name");
            return PartialView();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,CategoryTypeId")] Category category)
        {
            if (ModelState.IsValid)
            {
                category.HouseholdId = Convert.ToInt32(User.Identity.GetHouseholdId());
                db.Categories.Add(category);
                db.SaveChanges();
                if(Request.IsAjaxRequest())
                {
                    var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
                    ViewBag.CategoryId = new SelectList(hh.Categories, "Id", "Name");
                    return PartialView("CatDD");
                }
                return RedirectToAction("Index");
            }

            ViewBag.CategoryTypeId = new SelectList(db.CategoryTypes, "Id", "Name", category.CategoryTypeId);
            return View(category);
        }

        // GET: Categories/Edit/5
        public PartialViewResult Edit(int? id)
        {
            if (id == null)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            if (!hh.Categories.Contains(category)) // if category id does not belong to household - refuse access
                category = null;
            if (category == null)
            {
                //return HttpNotFound();
            }
            ViewBag.CategoryTypeId = new SelectList(db.CategoryTypes, "Id", "Name", category.CategoryTypeId);
            return PartialView(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,CategoryTypeId,HouseholdId")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryTypeId = new SelectList(db.CategoryTypes, "Id", "Name", category.CategoryTypeId);
            return View(category);
        }

        // GET: Categories/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Category category = db.Categories.Find(id);
        //    if (category == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(category);
        //}

        //  Categories/Delete/5
       
        public ActionResult Delete(int id)
        {
            Category category = db.Categories.Find(id);
            var cat = category;
            if (category.CategoryTypeId == 1) // 1 is income
            {
                cat = db.Categories.Where(c=>c.Name == "MiscIncome").FirstOrDefault();
            }
            else
            {
                 cat = db.Categories.Where(c=>c.Name == "MiscExpense").FirstOrDefault();
            }
            
            // change cat of budgetitems to Misc
            var bItems = db.BudgetItems.Where(b=>b.CategoryId == category.Id);
            foreach (var bi in bItems)
            {
                bi.CategoryId = cat.Id;
                db.Entry(bi).State = EntityState.Modified;
            }
            // change cat of transactions to Misc
            var trans = db.Transactions.Where(t=>t.CategoryId == category.Id);
            foreach (var tr in trans)
            {
                tr.CategoryId = cat.Id;
                db.Entry(tr).State = EntityState.Modified;
            }
            
            db.Categories.Remove(category);
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
