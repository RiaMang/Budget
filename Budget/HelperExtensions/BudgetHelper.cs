using Budget.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.HelperExtensions
{
    public static class BudgetHelper
    {
        public static ICollection<BudgetMod> GetBudget(this Household hh)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            //var month = System.DateTimeOffset.Now.Month;
            //var year = System.DateTimeOffset.Now.Year;
            var tod = System.DateTimeOffset.Now;
            //DateTimeOffset endDate = date.;
            //decimal totalBudget = 0;
            //decimal totalActual = 0;
            
            //var buds = (from c in db.Categories
            //           let biAmount = (from b in c.BudgetItems
            //                           select (b.Amount)).DefaultIfEmpty().Sum()
            //           let trAmount = (from t in c.Transactions
            //                           where t.TransDate.Month == tod.Month && t.TransDate.Year == tod.Year
            //                           select (t.Amount)).DefaultIfEmpty().Sum()
            //           let _ = totalBudget += biAmount
            //           let __ = totalActual += trAmount
            //           select new
            //           {
            //               Category = c.Name,
            //               Type = c.CategoryType.Name,
            //               EstAmount = biAmount,
            //               ActAmount = trAmount
            //           }).ToArray();
            
            //var Bud = new
            //{
            //    Budget = buds,
            //    totalBudget = totalBudget,
            //    totalActual = totalActual
            //};

            var cats = hh.Categories;
            List<BudgetMod> buds = new List<BudgetMod>();
            foreach (var cat in cats)
            {
                BudgetMod bud = new BudgetMod();
                bud.Category = cat.Name;
                bud.Type = cat.CategoryType.Name;
                var Bis = db.BudgetItems.Where(b => b.CategoryId == cat.Id);
                decimal total = 0;
                foreach (var bi in Bis)
                {
                    total += bi.Amount;
                }
                bud.EstAmount = total;
                total = 0;
                var trans = db.Transactions.Where(t => t.CategoryId == cat.Id && t.TransDate.Month == tod.Month && t.TransDate.Year == tod.Year);

                foreach (var tr in trans)
                {
                    total += tr.Amount;
                }
                bud.ActAmout = total;
                buds.Add(bud);
            }
            return buds;
            
        }
    }
}