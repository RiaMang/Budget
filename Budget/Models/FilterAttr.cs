using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

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

                if (!string.IsNullOrWhiteSpace(httpContext.User.Identity.GetHouseholdId()))
                {
                    return true;
                }
                else
                {
                    return false;
                }
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
    }
