using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FoodCo.Models;
using System.Data.Entity.Migrations;
using Microsoft.AspNet.Identity;

namespace FoodCo.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private ApplicationDbContext DB = new ApplicationDbContext();

        private readonly UserRepository _userRepository;

        public ChatController()
        {
            _userRepository = new UserRepository(new ApplicationDbContext());
        }

        // GET: ChatRoom
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetFriendsList(bool forceRefresh = false)
        {

            if (forceRefresh || OnLineUsers.HasChanged())
            {
                var sessionUser = User.Identity.GetUserId();

                if (sessionUser != null)
                {
                    var friends = new List<ApplicationUser>();
                    var users = DB.Users.ToList();
                    foreach (var friend in users)
                    {
                        if (!DB.Blocks.Where(b => b.BlockerId == friend.Id && b.BlockedId == sessionUser && b.BlockStatus == EnumBlockStatus.Blocked).Any() &&
                            !DB.Blocks.Where(b => b.BlockedId == friend.Id && b.BlockerId == sessionUser && b.BlockStatus == EnumBlockStatus.Blocked).Any() &&
                            (DB.Follows.Where(f => f.FollowerId == friend.Id && f.FollowedUserId == sessionUser && f.FriendshipStatus == EnumFriendshipStatus.Following).Any() ||
                            DB.Follows.Where(f => f.FollowerId == sessionUser && f.FollowedUserId == friend.Id && f.FriendshipStatus == EnumFriendshipStatus.Following).Any()))
                        {
                            friends.Add(friend);
                        }
                    }
                    if (friends.Count() > 0)
                    {
                        return PartialView(friends);
                    }
                }

                return null;
            }

            return null;
        }


        public JsonResult SetCurrentTarget(string userId)
        {
            if (_userRepository.Get(userId) != null)
            {
                Session["currentChatTarget"] = userId;

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetChatRoom(bool forceRefresh = false)
        {
            if (forceRefresh || OnLineUsers.HasChanged() || Chat.ChatHasChanged())
            {
                if (Session["currentChatTarget"] == null)
                {
                    return null;
                }

                string userId = OnLineUsers.GetSessionUser().Id;
                string friendId = (string)Session["currentChatTarget"];
                //User friend = DB.Users.Get(friendId);

                if (_userRepository.Get(userId) == null)
                {
                    return null;
                }


                var currentUserMessages = _userRepository.GetUsersChats(userId, friendId);
                var friendMessages = _userRepository.GetUsersChats(friendId, userId);


                return PartialView((currentUserMessages, friendMessages));
            }
            return null;
        }
        [HttpPost]
        public ActionResult SendMessage(string message)
        {
            string friendId = (string)Session["currentChatTarget"];
            string userId = OnLineUsers.GetSessionUser().Id;

            if (message.Length > 500)
            {
                return Json(new { success = false, error = "Le message doit contenir moins de 500 caractères" });
            }

            if (string.IsNullOrWhiteSpace(message) || _userRepository.Get(friendId) == null)
            {
                return Json(new { success = false, error = "Message invalide ou ami inexistant" });
            }

            try
            {
                Chat newMessage = new Chat
                {
                    UserId = userId,
                    ResponderId = friendId,
                    Message = message,
                    Date = DateTime.Now,
                    isModified = false,
                    IsRead = false,
                };
                DB.Chats.Add(newMessage);
                DB.SaveChanges();
                Chat.SetChatHasChanged();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "An error occurred while sending the message." });
            }
        }

        public ActionResult UpdateMessage(int messageId, string message)
        {
            string userId = OnLineUsers.GetSessionUser().Id;
            try
            {
                Chat msg = DB.Chats.FirstOrDefault(m=> m.Id == messageId);
                if (msg != null && msg.UserId == userId)
                {

                    msg.Message = message;
                    msg.isModified = true;


                    DB.Chats.AddOrUpdate(msg);
                    DB.SaveChanges();
                    Chat.SetChatHasChanged();
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false });
                }

            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "An error occurred while updating the message." });

            }

        }
        public ActionResult DeleteMessage(int messageId)
        {
            string userId = OnLineUsers.GetSessionUser().Id;

            try
            {
                Chat msg = DB.Chats.FirstOrDefault(m => m.Id == messageId);
                if (msg != null && msg.UserId == userId)
                {
                    DB.Chats.Remove(msg);
                    DB.SaveChanges();
                    Chat.SetChatHasChanged();
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false });
                }

            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Une erreur s'est produite lors de la suppression du nessage" });

            }

        }

        //[OnLineUsers.AdminAccess]
        //public ActionResult DeleteChat(int chatid)
        //{
        //    var user = OnLineUsers.GetSessionUser();
        //    try
        //    {
        //        var m = DB.Chats.Get(chatid);
        //        if (m != null && user.IsAdmin)
        //        {
        //            DB.UserChats.Delete(chatid);
        //            return Json(new { success = true });
        //        }
        //        else
        //        {
        //            return Json(new { success = false });
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, error = "Une erreur s'est produite lors de la suppression du nessage" });

        //    }
        //}

        public JsonResult GetMessage(int messageId)
        {
            string userId = OnLineUsers.GetSessionUser().Id;


            Chat msg = DB.Chats.FirstOrDefault(m => m.Id == messageId);
            if (msg != null && msg.UserId == userId)
            {
                var message = msg.Message;
                return Json(message, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false });
            }


        }
        //[OnLineUsers.AdminAccess]
        //public ActionResult Logs()
        //{
        //    return View();
        //}
        //[OnLineUsers.AdminAccess]
        //public ActionResult GetChatLogs(bool forceRefresh = false)
        //{
        //    if (forceRefresh || OnLineUsers.HasChanged() || DB.Users.HasChanged || DB.UserFriendships.HasChanged || DB.UserChats.HasChanged)
        //    {
        //        var chats = DB.UserChats.ToList().OrderByDescending(c => c.Date).ThenBy(c => c.Date.TimeOfDay);
        //        return PartialView(chats);
        //    }
        //    return null;
        //}

        //public JsonResult IsTyping()
        //{
        //    string friendId = (string)Session["currentChatTarget"];
        //    string userId = OnLineUsers.GetSessionUser().Id;
        //    ////var friendship = DB.Users.GetFriendShip(userId, friendId);
        //    //if (friendship != null && friendship.IsFriend && !friendship.isTyping)
        //    //{
        //    //    friendship.isTyping = true;
        //    //    DB.UserFriendships.Update(friendship);
        //    //    return Json(true, JsonRequestBehavior.AllowGet);
        //    //}
        //    return Json(false, JsonRequestBehavior.AllowGet);
        //}

        //public JsonResult StopTyping()
        //{
        //    int friendId = (int)Session["currentChatTarget"];
        //    int userId = OnLineUsers.GetSessionUser().Id;
        //    var friendship = DB.Users.GetFriendShip(userId, friendId);
        //    if (friendship != null && friendship.IsFriend && friendship.isTyping)
        //    {
        //        friendship.isTyping = false;
        //        DB.UserFriendships.Update(friendship);
        //        return Json(true, JsonRequestBehavior.AllowGet);
        //    }
        //    return Json(false, JsonRequestBehavior.AllowGet);
        //}

    //    public JsonResult IsTargetTyping()
    //    {
    //        int friendId = (int)Session["currentChatTarget"];
    //        int userId = OnLineUsers.GetSessionUser().Id;
    //        var friendship = DB.Users.GetFriendShip(friendId, userId);
    //        if (friendship != null && friendship.IsFriend && friendship.isTyping)
    //        {
    //            return Json(true, JsonRequestBehavior.AllowGet);
    //        }
    //        return Json(false, JsonRequestBehavior.AllowGet);
    //    }
    }
}
