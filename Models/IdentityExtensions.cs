using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace FoodCo.Models
{
    public static class IdentityExtensions
    {
        public static string GetProfilePicturePath(this IIdentity identity)
        {
            var claimsIdentity = identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var profilePictureClaim = claimsIdentity.FindFirst("ProfilePicturePath");
                if (profilePictureClaim != null)
                {
                    return profilePictureClaim.Value;
                }
            }

            return null;
        }
    }

}