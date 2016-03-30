using Budget.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Net;
using System.Data.Entity;
using System.IO;
using DataTables.Mvc;

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


        public JsonResult GetTransactions([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest request, int id)
        {
            var hh = db.Households.Find(User.Identity.GetHouseholdId<int>());
            var account = db.Accounts.Find(id);
            //var trans = hh.Accounts.SelectMany(a => a.Transactions);
            var trans = account.Transactions.AsQueryable();
            var totalCount = trans.Count();
            var search = request.Search.Value;


            if (!string.IsNullOrWhiteSpace(search))
            {
                trans = trans.Where(t => t.Amount.ToString().Contains(search) || t.Description.Contains(search)
                    || t.RecAmount.ToString().Contains(search) || t.UpdatedBy.Contains(search) || t.Category.Name.Contains(search));

            }


            trans = trans.OrderByDescending(t => t.TransDate);

            var column = request.Columns.FirstOrDefault(r => r.IsOrdered == true);
            if (column != null)
            {
                if (column.SortDirection == Column.OrderDirection.Descendant)
                {
                    switch (column.Data)
                    {
                        case "Title":
                            trans = trans.OrderByDescending(t => t.TransDate);
                            break;
                        case "Description":
                            trans = trans.OrderByDescending(t => t.Description);
                            break;
                        case "Category":
                            trans = trans.OrderByDescending(t => t.Category.Name);
                            break;
                        
                        case "Amount":
                            trans = trans.OrderByDescending(t => t.Amount);
                            break;
                        case "Reconciled":
                            trans = trans.OrderByDescending(t => t.RecAmount);
                            break;
                        
                        case "UpdatedBy":
                            trans = trans.OrderByDescending(t => t.UpdatedBy);
                            break;
                        
                    }
                }
                else
                {
                    switch (column.Data)
                    {
                        case "Title":
                            trans = trans.OrderBy(t => t.TransDate);
                            break;
                        case "Description":
                            trans = trans.OrderBy(t => t.Description);
                            break;
                        case "Category":
                            trans = trans.OrderBy(t => t.Category.Name);
                            break;

                        case "Amount":
                            trans = trans.OrderBy(t => t.Amount);
                            break;
                        case "Reconciled":
                            trans = trans.OrderBy(t => t.RecAmount);
                            break;

                        case "UpdatedBy":
                            trans = trans.OrderBy(t => t.UpdatedBy);
                            break;
                    }
                }
            }

            //var filteredColumns = request.Columns.GetFilteredColumns();
            //foreach (var col in filteredColumns)
            //{
            //     Filter(column.Data, column.Search.Value, column.Search.IsRegexValue); 
            //}

            var uug = trans.Skip(request.Start).Take(request.Length);
            var paged = uug.Select(t => new TrViewModel
            {
                Id = t.Id,
                AccountId = t.AccountId,
                TransDate = t.TransDate,
                Description = t.Description,
                Category = t.Category.Name,
                Amount = t.Amount,
                Reconciled = t.RecAmount,
                UpdatedBy = t.UpdatedBy,
                Edit = "<a class=\"btn btn-default\" href=\"/Transactions/Edit/"+ t.Id + "\"><i class=\"icon glyphicon glyphicon-edit\"></i></a>",
                Delete = "<button class=\"buton btn btn-default\" data-toggle=\"modal\" data-id=\""+t.Id+"\" data-target=\"#DeleteModal\" data-description=\""+t.Description+"\" data-amount=\""+t.Amount+"\"><i class=\"icon glyphicon glyphicon-trash\"></i></button>",
                });
            //return Json(new DataTablesResponse(request.Draw, paged, tickets.Count(), totalCount), JsonRequestBehavior.AllowGet);
            
            return Json(new DataTablesResponse(request.Draw, paged, trans.Count(),totalCount), JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult _Transactions(int id)
        {
            var acc = db.Accounts.Find(id);
            return PartialView(acc.Transactions.ToList());
        }

        //get Create Transactions
        public ActionResult Create(int id)
        {
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            ViewBag.CategoryId = new SelectList(hh.Categories, "Id", "Name");
            ViewBag.CategoryTypeId = new SelectList(db.CategoryTypes, "Id", "Name");
            TempData["AccId"] = id;
            return View();
        }

        //get Create Transactions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Transaction tr)
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
            return View(tr);
            //return RedirectToAction("Details", "Accounts", new { Id = accId, tr });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(int? id, HttpPostedFileBase fileCSV)
        {
            var hh = db.Households.Find(User.Identity.GetHouseholdId<int>());
            if (fileCSV != null && fileCSV.ContentLength > 0)
            {
                //check the file name to make sure its an image
                var ext = Path.GetExtension(fileCSV.FileName).ToLower();
                
                if (ext != ".csv")
                    ModelState.AddModelError("fileCSV", "Invalid Format."); // throw an error
            }
            if (ModelState.IsValid)
            {
                if (fileCSV != null)
                {
                    var filePath = "/Uploads/";
                    var absPath = Server.MapPath("~" + filePath);
                    var FileUrl = filePath + fileCSV.FileName;
                    ////save image
                    filePath = Path.Combine(absPath, fileCSV.FileName);
                    fileCSV.SaveAs(filePath);
                    var fileContent = System.IO.File.ReadAllText(filePath);
                    //char[] delim = {'\r', '\n'};
                    string [] records = fileContent.Split('\r','\n');
                    string[] fields={" "," "};
                    var acc = db.Accounts.Find(id);
                    var balance = acc.Balance;
                    foreach (var rec in records)
                    {
                        if(rec!="")
                        {
                            Transaction tr = new Transaction();
                            tr.TransDate = System.DateTimeOffset.Now;
                            tr.AccountId = (int)id;
                            var user = db.Users.Find(User.Identity.GetUserId());
                            tr.UpdatedBy = user.Name;
                             
                            fields = rec.Split(',');
                            tr.Description = fields[0];
                            tr.Amount = Convert.ToDecimal(fields[1]);
                            tr.RecAmount = Convert.ToDecimal(fields[1]);
                            var type = fields[3];
                            var Category = hh.Categories.FirstOrDefault(c=>c.Name == fields[2]);
                            if (Category != null)
                                tr.CategoryId = Category.Id;
                            else
                            {
                                if (type == "Expense")
                                    tr.CategoryId = hh.Categories.FirstOrDefault(z => z.Name == "MiscExpense").Id;
                                else
                                    tr.CategoryId = hh.Categories.FirstOrDefault(z => z.Name == "MiscIncome").Id;
                            }
                               
                               
                            db.Transactions.Add(tr);
                           
                            var cat = db.Categories.Find(tr.CategoryId);
                            if (cat.CategoryType.Name == "Expense") // Expense
                            {
                                balance -= tr.Amount;
                            }
                            else
                            {
                                balance += tr.Amount;
                            }
                            
                            //db.SaveChanges();
                        }

                    }
                    
                    acc.Balance = balance;
                    db.Entry(acc).Property("Balance").IsModified = true;
                    db.SaveChanges();

                    return RedirectToAction("Details", "Accounts", new { id = id });
                }
            }
            return Json(" ", JsonRequestBehavior.AllowGet);
        }
        
        
        // GET: Transactions/Edit/5
        
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                //return PartialView(null);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Transaction tr = db.Transactions.Find(id);
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            
            if (tr == null)
            {
                //return PartialView(null);
                return HttpNotFound();
            }
            else
            {
                var acc = db.Accounts.Find(tr.AccountId); // if Account does not belong to household, refuse access.
                if (!hh.Accounts.Contains(acc))
                {
                    //tr = null;
                    return HttpNotFound();
                }
            }
            
            ViewBag.CategoryId = new SelectList(hh.Categories, "Id", "Name", tr.CategoryId);
            ViewBag.AccountId = new SelectList(hh.Accounts, "Id", "Name", tr.AccountId);
            //return PartialView("_Edit",tr);
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

                //oldTr = db.Transactions.AsNoTracking().FirstOrDefault(c=> c.Id == tr.Id);

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

        // GET: Transactions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction tr = db.Transactions.Find(id);
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            
            if (tr == null)
            {
                return HttpNotFound();
            }
            else
            {
                var acc = db.Accounts.Find(tr.AccountId); // if Account does not belong to household, refuse access.
                if (!hh.Accounts.Contains(acc))
                {
                    tr = null;
                    return HttpNotFound();
                }
            }
            return View(tr);
        }

        // POST: Transactions/Delete/5
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

        [Route ("TransactionsByType(Income/Expense)")]
        public ActionResult TransByType()
        {
            TransByTypeViewModel tbt = new TransByTypeViewModel();
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            tbt.Types = db.CategoryTypes.ToList();
            tbt.Categories = hh.Categories.ToList();
            var tod = System.DateTimeOffset.Now;
            tbt.Transactions = hh.Accounts.SelectMany(t => t.Transactions).Where(d=>d.TransDate.Year==tod.Year && d.TransDate.Month == tod.Month).OrderByDescending(a=>a.TransDate).ToList();
            return View(tbt);
        }

        [Route ("TransactionsByCategory")]
        public ActionResult TransByCat()
        {
            TransByCatViewModel tcm = new TransByCatViewModel();
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            tcm.Categories = hh.Categories.ToList();
            var tod = System.DateTimeOffset.Now;
            tcm.Transactions = hh.Accounts.SelectMany(t => t.Transactions).Where(d => d.TransDate.Year == tod.Year && d.TransDate.Month == tod.Month).OrderByDescending(a=>a.TransDate).ToList();
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