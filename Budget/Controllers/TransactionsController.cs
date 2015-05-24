﻿using Budget.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Net;
using System.Data.Entity;

namespace Budget.Controllers
{
    
    [AuthorizeHouseholdRequired]
    public class TransactionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        
        // GET: Transactions
        public ActionResult Index()
        {
            return View();
        }
        
        public PartialViewResult _Transactions(int id)
        {
            var acc = db.Accounts.Find(id);
            return PartialView(acc.Transactions.ToList());
        }

        //get Create Transactions
        public PartialViewResult _AddTrans(int id)
        {
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            ViewBag.CategoryId = new SelectList(hh.Categories, "Id", "Name");
            TempData["AccId"] = id;
            return PartialView();
        }

        //get Create Transactions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _AddTrans(Transaction tr)
        {
            var accId = (int)TempData["AccId"];
            if(ModelState.IsValid)
            { 
                tr.AccountId = accId;
                var user = db.Users.Find(User.Identity.GetUserId());
                tr.UpdatedBy = user.Name;
                db.Transactions.Add(tr);
                var acc = db.Accounts.Find(accId);
                var cat = db.Categories.Find(tr.CategoryId);
                if(cat.CategoryTypeId == 2) // Expense
                {
                    acc.Balance -= tr.Amount;
                } else
                {
                    acc.Balance += tr.Amount;
                }
                db.Entry(acc).Property("Balance").IsModified = true;
                db.SaveChanges();
                if(acc.Balance <100)
                {
                    ViewBag.Note = "Your account balance is less than $100.";
                }
                return RedirectToAction("Details", "Accounts", new { id = accId});
            }
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            ViewBag.CategoryId = new SelectList(hh.Categories, "Id", "Name",tr.CategoryId);
            //return PartialView("_AddTrans", tr);
            return RedirectToAction("Details", "Accounts", new { Id = accId, tr });
        }

        // GET: Accounts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            Transaction tr = db.Transactions.Find(id);
            if (tr == null)
            {
                return HttpNotFound();
            }
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            ViewBag.CategoryId = new SelectList(hh.Categories, "Id", "Name", tr.CategoryId);
            ViewBag.AccountId = new SelectList(hh.Accounts, "Id", "Name", tr.AccountId);
            return View(tr);
        }

        // POST: Accounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Description,TransDate,Amount,RecAmount,CategoryId,AccountId")] Transaction tr)
        {
            
            if (ModelState.IsValid)
            {
                var oldTr = (from t in db.Transactions.AsNoTracking()
                            where t.Id == tr.Id
                            select t).FirstOrDefault();
                var acc = db.Accounts.Find(oldTr.AccountId);
                var cat = db.Categories.Find(oldTr.CategoryId);
                if (cat.CategoryTypeId == 2) // Expense
                {
                    acc.Balance += oldTr.Amount;
                }
                else
                {
                    acc.Balance -= oldTr.Amount;
                }                               // reversing the old transaction

                tr.UpdatedBy = db.Users.Find(User.Identity.GetUserId()).Name;
                db.Entry(tr).State = EntityState.Modified;      // Edit the transaction record 
                //db.Accounts.Attach(acc);
                db.Entry(acc).State = EntityState.Modified;     // update the old account balance
                db.SaveChanges();                           

                acc = db.Accounts.Find(tr.AccountId);
                cat = db.Categories.Find(tr.CategoryId);
                if (cat.CategoryTypeId == 2) // Expense
                {
                    acc.Balance -= tr.Amount;
                }
                else
                {
                    acc.Balance += tr.Amount;
                }                       // new transaction updates account balance

                //db.Accounts.Attach(acc);
                db.Entry(acc).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Details", "Accounts", new { id = tr.AccountId});
            }
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            ViewBag.CategoryId = new SelectList(hh.Categories, "Id", "Name", tr.CategoryId);
            ViewBag.AccountId = new SelectList(hh.Accounts, "Id", "Name", tr.AccountId);
            return View(tr);
        }

        // GET: Accounts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction tr = db.Transactions.Find(id);
            if (tr == null)
            {
                return HttpNotFound();
            }
            return View(tr);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Transaction tr = db.Transactions.Find(id);
            var acc = db.Accounts.Find(tr.AccountId);
            var cat = db.Categories.Find(tr.CategoryId);
            if (cat.CategoryTypeId == 2) // Expense
            {
                acc.Balance += tr.Amount;
            }
            else
            {
                acc.Balance -= tr.Amount;
            }                                   // Reversing the account balance

            db.Entry(acc).State = EntityState.Modified;
            db.Transactions.Remove(tr);
            db.SaveChanges();
            return RedirectToAction("Details", "Accounts", new { id = acc.Id});
        }

        public ActionResult TransByType()
        {
            TransByTypeViewModel tbt = new TransByTypeViewModel();
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            tbt.Types = db.CategoryTypes.ToList();
            tbt.Categories = hh.Categories.ToList();
            tbt.Transactions = hh.Accounts.SelectMany(t => t.Transactions).ToList();
            return View(tbt);
        }

        public ActionResult TransByCat()
        {
            TransByCatViewModel tcm = new TransByCatViewModel();
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            tcm.Categories = hh.Categories.ToList();
            tcm.Transactions = hh.Accounts.SelectMany(t => t.Transactions).ToList();
            return View(tcm);
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