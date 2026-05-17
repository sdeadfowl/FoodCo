using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FoodCo.Models;

namespace FoodCo.Controllers
{
    public class NotificationsController: Controller
    {
        // GET: Notifications
        public JsonResult Pop()
        {
            ApplicationUser loggedUser = OnLineUsers.GetSessionUser();
            List<string> messages = new List<string>();
            if (loggedUser != null)
            {
                messages = OnLineUsers.PopNotifications(loggedUser.Id);
            }
            return Json(messages, JsonRequestBehavior.AllowGet);
        }
    }
}