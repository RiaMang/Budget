﻿using System;
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
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Categories/Create
        public ActionResult Create()
        {
            ViewBag.CategoryTypeId = new SelectList(db.CategoryTypes, "Id", "Name");
            return View();
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
                return RedirectToAction("Index");
            }

            ViewBag.CategoryTypeId = new SelectList(db.CategoryTypes, "Id", "Name", category.CategoryTypeId);
            return View(category);
        }

        // GET: Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryTypeId = new SelectList(db.CategoryTypes, "Id", "Name", category.CategoryTypeId);
            return View(category);
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