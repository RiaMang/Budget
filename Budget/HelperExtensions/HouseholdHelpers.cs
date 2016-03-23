using Budget.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Budget.HelperExtensions;
using System.Text;

namespace Budget.HelperExtensions
{
    public static class HouseholdHelpers
    {
       
        
        public static ICollection<ApplicationUser> UsersInHousehold(this Household h)
        {
            return h.Users.ToList();
        }
        

        public static void AddCategories(this Household h)
        {
            Category[] c ={
                new Category { Name = "Salary", CategoryTypeId = 1, HouseholdId = h.Id, }, //type 1 = Income, 2= Expense
                new Category {Name = "Bills", CategoryTypeId = 2, HouseholdId = h.Id, },
                new Category {Name = "Transportation", CategoryTypeId = 2, HouseholdId = h.Id, },
                 new Category {Name = "Food", CategoryTypeId = 2, HouseholdId = h.Id, },
                  new Category {Name = "Rent", CategoryTypeId = 2, HouseholdId = h.Id, },
                  new Category { Name = "MiscIncome", CategoryTypeId = 1, HouseholdId = h.Id, }, //type 1 = Income, 2= Expense
                new Category {Name = "MiscExpense", CategoryTypeId = 2, HouseholdId = h.Id, }
              };
            db.Categories.AddRange(c);
            db.SaveChanges();
        }

        private static ApplicationDbContext db = new ApplicationDbContext();

        public static void InviteUserToHousehold(this Invitation invite, ApplicationUser user)
        {
            invite.Code = " ".genRandom();
            
            invite.HouseholdId = user.HouseholdId;
            db.Invitations.Add(invite);
            db.SaveChanges();
            // send email to this user.
            user.SendEmail("Invitation to Household : "+user.Household.Name, "You have been invited to join the Household : " + user.Household.Name + 
                ". Enter the code: " + invite.Code + 
                " after you register and sign in, to join the household.");
        }

        public static void SendEmail(this ApplicationUser user, string sub, string txt)
        {
            EmailService es = new EmailService();
            es.SendAsync(new IdentityMessage { Destination=user.Email, Subject=sub, Body=txt});
        }

        public static bool RemoveUserFromHousehold(this IDbSet<ApplicationUser> users, string Id)  //signin after this 
        {
            var usr = users.Include(p=>p.Household).FirstOrDefault(u => u.Id == Id);
            var h = usr.Household;
            
            if (usr.Household.Users.Count() == 1)
            {
                
                    //var Bi = h.BudgetItems.ToList();
                    //db.BudgetItems.RemoveRange(Bi);

                    //db.Accounts.RemoveRange(h.Accounts);
                
                    ////db.Households.Find(h.Id);
                    //db.Households.Remove(h);
                    h.RemovedDate = System.DateTimeOffset.Now;
                    //db.Households.SoftDelete(h.Id);
                    //db.Entry(h).State = EntityState.Modified;
                    usr.HouseholdId = null;
                    //db.Entry(usr).State = EntityState.Modified;
                    
            }
            else
            {
                usr.HouseholdId = null;
                //db.Users.Attach(user);
                //db.Entry(usr).Property(p => p.HouseholdId).IsModified = true;
                
            }
            db.SaveChanges();
            return true;
        }




        //public static void AddCategories(this Household h)
        //{
        //    Category[] c ={
        //        new Category { Name = "Salary", CategoryTypeId = 1, HouseholdId = h.Id, }, //type 1 = Income, 2= Expense
        //        new Category {Name = "Bills", CategoryTypeId = 2, HouseholdId = h.Id, },
        //        new Category {Name = "Transportation", CategoryTypeId = 2, HouseholdId = h.Id, },
        //         new Category {Name = "Food", CategoryTypeId = 2, HouseholdId = h.Id, },
        //          new Category {Name = "Rent", CategoryTypeId = 2, HouseholdId = h.Id, },
        //          new Category { Name = "MiscIncome", CategoryTypeId = 1, HouseholdId = h.Id, }, //type 1 = Income, 2= Expense
        //        new Category {Name = "MiscExpense", CategoryTypeId = 2, HouseholdId = h.Id, }
        //      };
        //    db.Categories.AddRange(c);
        //    db.SaveChanges();
        //}


        public static string genRandom(this string s)
        {
            // selected characters
            string chars = "2346789ABCDEFGHJKLMNPQRTUVWXYZabcdefghjkmnpqrtuvwxyz-@#$%^&*()!~";
            // create random generator
            Random rnd = new Random();

            // create name
            var name = new StringBuilder();
            while (name.Length < 7)
            {
                name.Append( chars[rnd.Next(chars.Length)] );
            }
            return name.ToString();
        }





    }
}