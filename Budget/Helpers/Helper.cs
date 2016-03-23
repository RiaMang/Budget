using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.Models
{
    
    public static class Help
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        public static int GetHouseholdId(this string userId)
        {
            return (int)db.Users.Find(userId).HouseholdId;
        }
    }
    




    public class Helper
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public int GetHousehold(string userId)
        {
                     
            return (int)db.Users.Find(userId).HouseholdId;
          
        }
    }
}

