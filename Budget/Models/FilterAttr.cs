using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Budget.Models
{
    public class FilterAttr
    {
    }
    public static class Extensions
    {
        public static string GetHouseholdId(this IIdentity user)
        {
            var claimsIdentity = (ClaimsIdentity)user;
            var HouseholdClaim = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "HouseholdId");
            if (HouseholdClaim != null)
                return HouseholdClaim.Value;
            else
                return null;
        }

        public static string GetName(this IIdentity user)
        {
            var claimsIdentity = (ClaimsIdentity)user;
            var NameClaim = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Name");
            if (NameClaim != null)
                return NameClaim.Value;
            else
                return null;
        }

        public static T GetHouseholdId<T>(this IIdentity user)
        {
            var claimsIdentity = (ClaimsIdentity)user;
            var HouseholdClaim = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "HouseholdId");
            if (HouseholdClaim != null)
                return (T)Convert.ChangeType(HouseholdClaim.Value, typeof(T));
            else
                return default(T);
        }

        public static bool IsInHousehold(this IIdentity user)
        {
            var cUser = (ClaimsIdentity)user;
            var hid = cUser.Claims.FirstOrDefault(c => c.Type == "HouseholdId");
            return (hid != null && !string.IsNullOrWhiteSpace(hid.Value));
        }
    }

        public class AuthorizeHouseholdRequired : AuthorizeAttribute
        {
            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                var isAuthorized = base.AuthorizeCore(httpContext);
                if (!isAuthorized)
                {
                    return false;
                }

                return httpContext.User.Identity.IsInHousehold();

                //if (!string.IsNullOrWhiteSpace(httpContext.User.Identity.GetHouseholdId()))
                //{
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
            }

            protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
            {
                if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    base.HandleUnauthorizedRequest(filterContext);
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(new
                    RouteValueDictionary(new { controller = "Home", action = "CreateJoinHousehold" }));
                }
            }
        }

    public static class AuthExtensions
    {
        public static async Task RefreshAuthentication(this HttpContextBase context, ApplicationUser user)
        {
            context.GetOwinContext().Authentication.SignOut();
            await context.GetOwinContext().Get<ApplicationSignInManager>().SignInAsync(user, isPersistent: false, rememberBrowser: false);
        }
    }
    }
