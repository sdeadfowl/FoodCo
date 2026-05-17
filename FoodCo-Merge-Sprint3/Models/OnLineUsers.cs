using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FoodCo.Models
{
    public static class OnLineUsers
    {

        public static List<string> ConnectedUsersId
        {
            get
            {
                if (HttpRuntime.Cache["OnLineUsers"] == null)
                    HttpRuntime.Cache["OnLineUsers"] = new List<string>();
                return (List<string>)HttpRuntime.Cache["OnLineUsers"];
            }
        }
        #region Status change management
        private static string SerialNumber
        {
            get
            {
                if (HttpRuntime.Cache["OnLineUsersSerialNumber"] == null)
                    SetHasChanged();
                return (string)HttpRuntime.Cache["OnLineUsersSerialNumber"];
            }
            set
            {
                HttpRuntime.Cache["OnLineUsersSerialNumber"] = value;
            }
        }
        public static bool HasChanged()
        {
            if (HttpContext.Current.Session["SerialNumber"] == null)
            {
                HttpContext.Current.Session["SerialNumber"] = SerialNumber;
                return true;
            }
            string sessionSerialNumber = (string)HttpContext.Current.Session["SerialNumber"];
            HttpContext.Current.Session["SerialNumber"] = SerialNumber;
            return SerialNumber != sessionSerialNumber;
        }
        public static void SetHasChanged()
        {
            SerialNumber = Guid.NewGuid().ToString();
        }
        #endregion
        #region Session management
        public static void AddSessionUser(string userId)
        {
            HttpContext.Current.Session["UserId"] = userId;
            ConnectedUsersId.Add(userId);
            SetHasChanged();
        }
        public static void RemoveSessionUser()
        {
            ApplicationUser user = GetSessionUser();
            if (user != null)
                ConnectedUsersId.Remove(user.Id);
            HttpContext.Current?.Session.Abandon();
            SetHasChanged();
        }
        public static bool IsOnLine(string userId)
        {
            return ConnectedUsersId.Contains(userId);
        }
        public static ApplicationUser GetSessionUser()
        {
            using (var DB = new ApplicationDbContext())
            {
                if (HttpContext.Current.Session["UserId"] != null)
                {
                    ApplicationUser currentUser = DB.Users.Find((string)HttpContext.Current.Session["UserId"]);
                    return currentUser;
                }
                return null;
            }
        }
        //public static bool Write_Access()
        //{
        //    ApplicationUser sessionUser = OnLineUsers.GetSessionUser();
        //    if (sessionUser != null)
        //    {
        //        return sessionUser.IsPowerUser || sessionUser.IsAdmin;
        //    }
        //    return false;
        //}
        #endregion
        #region Notifications handling
        private static List<Notifications> Notifications
        {
            get
            {
                if (HttpRuntime.Cache["Notifications"] == null)
                    HttpRuntime.Cache["Notifications"] = new List<Notifications>();
                return (List<Notifications>)HttpRuntime.Cache["Notifications"];
            }
        }
        public static void AddNotification(string TargetUserId, string Message)
        {
            using (var DB = new ApplicationDbContext())
            {
                ApplicationUser user = DB.Users.Find(TargetUserId);
                if (user != null && IsOnLine(user.Id))
                {
                    Notifications.Add(new Notifications() { TargetUserId = TargetUserId, Message = Message });
                }
            }
        }
        public static List<string> PopNotifications(string TargetUserId)
        {
            List<string> notificationMessages = new List<string>();

            List<Notifications> notifications = Notifications
                .Where(n => n.TargetUserId == TargetUserId)
                .OrderBy(n => n.Created)
                .ToList();

            foreach (Notifications notification in notifications)
            {
                if (IsOnLine(notification.TargetUserId))
                {
                    notificationMessages.Add(notification.Message);
                }

                Notifications.Remove(notification);
            }

            return notificationMessages;
        }

        #endregion
        //#region Access control
        //public class UserAccess : AuthorizeAttribute
        //{
        //    private bool ServerSideResponseHandling { get; set; }
        //    public UserAccess(bool serverSideResponseHandling = true)
        //    {
        //        ServerSideResponseHandling = serverSideResponseHandling;
        //    }
        //    protected override bool AuthorizeCore(HttpContextBase httpContext)
        //    {
        //        ApplicationUser sessionUser = OnLineUsers.GetSessionUser();
        //        if (sessionUser != null)
        //        {
        //            if (sessionUser.Blocked)
        //            {
        //                RemoveSessionUser();
        //                if (ServerSideResponseHandling)
        //                {
        //                    httpContext.Response.Redirect("~/Accounts/Login?message=Compte bloqué!");
        //                    return false;
        //                }
        //                else
        //                {
        //                    httpContext.Response.StatusCode = 403; // Forbiden status
        //                }
        //            }
        //            return true;
        //        }
        //        httpContext.Response.Redirect("~/Accounts/Login?message=Accès non autorisé!");
        //        return false;

        //    }
        //}
        //public class PowerUserAccess : AuthorizeAttribute
        //{
        //    protected override bool AuthorizeCore(HttpContextBase httpContext)
        //    {
        //        User sessionUser = OnLineUsers.GetSessionUser();
        //        if (sessionUser != null && (sessionUser.IsPowerUser || sessionUser.IsAdmin))
        //            return true;
        //        else
        //        {
        //            httpContext.Response.Redirect("~/Accounts/Login?message=Accès non autorisé!", true);
        //        }
        //        return false;
        //    }
        //}
        //public class AdminAccess : AuthorizeAttribute
        //{
        //    protected override bool AuthorizeCore(HttpContextBase httpContext)
        //    {
        //        User sessionUser = OnLineUsers.GetSessionUser();
        //        if (sessionUser != null && sessionUser.IsAdmin)
        //            return true;
        //        else
        //        {
        //            httpContext.Response.Redirect("~/Accounts/Login?message=Accès non autorisé!");
        //        }
        //        return true;
        //    }
        //}
        //#endregion
    }

}