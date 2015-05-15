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

namespace Budget.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [AuthorizeHouseholdRequired]
        public ActionResult Index()
        {
            return View();
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
            var userid = User.Identity.GetUserId();
            
            return View(db.Households.Find(db.Users.Find(userid).HouseholdId));
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
            var userid = User.Identity.GetUserId();
            var user = db.Users.Find(userid);
            if (hvm.Code == null)
            {
                if (hvm.Name == null)
                    return View();
                Household h = new Household { Name = hvm.Name };
                db.Households.Add(h);
                //db.SaveChanges();
                user.HouseholdId = h.Id;
            }
            else
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
            ApplicationSignInManager SignInManager = HttpContext.GetOwinContext().Get<ApplicationSignInManager>();

            HttpContext.GetOwinContext().Authentication.SignOut();
            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

            db.SaveChanges();

            return RedirectToAction("Index");
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

            return RedirectToAction("Index");
        }



        [AuthorizeHouseholdRequired]
        public async Task<ActionResult> LeaveHousehold()
        {
            var userid = User.Identity.GetUserId();
            var user = db.Users.Find(userid);
            db.Users.RemoveUserFromHousehold(user.Id);
            ApplicationSignInManager SignInManager = HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            
            HttpContext.GetOwinContext().Authentication.SignOut();
            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

            return RedirectToAction("Household");
        }
    }
}