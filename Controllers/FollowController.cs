using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using FoodCo.Models;
using Mail;
using System.IO;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Web.Services.Description;
using System.Data.Entity.Migrations;

namespace FoodCo.Controllers
{
    public class FollowController : Controller
    {
        private ApplicationDbContext DB = new ApplicationDbContext();
        private readonly UserRepository _userRepository;

        public FollowController()
        {
            _userRepository = new UserRepository(new ApplicationDbContext());
        }
        private string SearchText
        {
            get
            {
                if (Session["SearchText"] == null)
                    Session["SearchText"] = "";
                return (string)Session["SearchText"];
            }
            set { Session["SearchText"] = value; }
        }
        private bool FilterNotFriend
        {
            get
            {
                if (Session["FilterNotFriend"] == null)
                    Session["FilterNotFriend"] = true;
                return (bool)Session["FilterNotFriend"];
            }
            set { Session["FilterNotFriend"] = value; }
        }
        private bool FilterRequest
        {
            get
            {
                if (Session["FilterRequest"] == null)
                    Session["FilterRequest"] = true;
                return (bool)Session["FilterRequest"];
            }
            set { Session["FilterRequest"] = value; }
        }
        private bool FilterFollow
        {
            get
            {
                if (Session["FilterFollow"] == null)
                    Session["FilterFollow"] = true;
                return (bool)Session["FilterFollow"];
            }
            set { Session["FilterFollow"] = value; }
        }
        private bool FilterFriend
        {
            get
            {
                if (Session["FilterFriend"] == null)
                    Session["FilterFriend"] = true;
                return (bool)Session["FilterFriend"];
            }
            set { Session["FilterFriend"] = value; }
        }
        private bool FilterUnFollow
        {
            get
            {
                if (Session["FilterUnFollow"] == null)
                    Session["FilterUnFollow"] = true;
                return (bool)Session["FilterUnFollow"];
            }
            set { Session["FilterUnFollow"] = value; }
        }
        private bool FilterBlocked
        {
            get
            {
                if (Session["FilterBlocked"] == null)
                    Session["FilterBlocked"] = true;
                return (bool)Session["FilterBlocked"];
            }
            set { Session["FilterBlocked"] = value; }
        }

        public ActionResult Index()
        {
            ViewBag.SearchText = SearchText;
            ViewBag.FilterRequest = FilterRequest;
            ViewBag.FilterNotFriend = FilterNotFriend;
            ViewBag.FilterFollow = FilterFollow;
            ViewBag.FilterFriend = FilterFriend;
            ViewBag.FilterUnFollow = FilterUnFollow;
            ViewBag.FilterBlocked = FilterBlocked;
            Session["LastAction"] = "/Friendships/index";
            return View();
        }

        public ActionResult Search(string text)
        {
            SearchText = text;
            return null;
        }

        public ActionResult SetFilterNotFriend(bool check)
        {
            FilterNotFriend = check;
            return null;
        }
        public ActionResult SetFilterRequest(bool check)
        {
            FilterRequest = check;
            return null;
        }
        public ActionResult SetFilterPending(bool check)
        {
            FilterFollow = check;
            return null;
        }
        public ActionResult SetFilterFriend(bool check)
        {
            FilterFriend = check;
            return null;
        }
        public ActionResult SetFilterUnFollow(bool check)
        {
            FilterUnFollow = check;
            return null;
        }
        public ActionResult SetFilterBlocked(bool check)
        {
            FilterBlocked = check;
            return null;
        }
        private List<ApplicationUser> FilterUsers()
        {
            FollowRepository followRepository = new FollowRepository();
            var loggedUser = OnLineUsers.GetSessionUser();
            List<ApplicationUser> filteredUsers = new List<ApplicationUser>();
            List<ApplicationUser> users = DB.Users.ToList();
            foreach (ApplicationUser user in users)
            {
                bool keep = true;
                switch (followRepository.RelationStatus(loggedUser.Id, user.Id))
                {
                    case EnumRelationStatus.NotFriend: /* not friend relation*/
                        keep = FilterNotFriend;
                        break;
                    case EnumRelationStatus.Friend: /* friend*/
                        keep = FilterFriend;
                        break;
                    case EnumRelationStatus.UnFollow: /* friendship declined*/
                        keep = FilterUnFollow;
                        break;
                    case EnumRelationStatus.Following: /* friendship pending*/
                        keep = FilterFollow;
                        break;
                    case EnumRelationStatus.Follower: /* friendship request*/
                        keep = FilterRequest;
                        break;
                    case EnumRelationStatus.Blocked: /* blocked*/
                        keep = FilterBlocked;
                        break;
                    default: break;
                }
                if (keep) filteredUsers.Add(user);
            }
            return filteredUsers;
        }

        public ActionResult GetFollowStatus(bool forceRefresh = false)
        {
            if (forceRefresh || DB.FollowRepositories.HasChanged || OnLineUsers.HasChanged())
                return PartialView(FilterUsers());

            return null;
        }
        [HttpPost]
        public JsonResult Following(string id)
        {
            ApplicationUser currentUser = OnLineUsers.GetSessionUser();
            ApplicationUser followedUser = _userRepository.Get(id);
            Follow follow = new Follow()
            {
                FollowerId = currentUser.Id,
                FollowedUserId = followedUser.Id,
                CreationDate = DateTime.Now,
                FriendshipStatus = EnumFriendshipStatus.Following,
            };
            DB.Follows.AddOrUpdate(follow);
            DB.SaveChanges();
            return Json(follow, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UnFollow(string id)
        {
            ApplicationUser currentUser = OnLineUsers.GetSessionUser();
            ApplicationUser unfollowedUser = _userRepository.Get(id);
            Follow oldRelation = DB.Follows.Find(currentUser.Id, unfollowedUser.Id);
            Follow follow = new Follow()
            {
                FollowerId = currentUser.Id,
                FollowedUserId = unfollowedUser.Id,
                CreationDate = oldRelation.CreationDate,
                FriendshipStatus = EnumFriendshipStatus.UnFollow,
            };
            DB.Follows.AddOrUpdate(follow);
            DB.SaveChanges();
            return Json(follow, JsonRequestBehavior.AllowGet);
        }
        public ActionResult FollowBack(string id)
        {
            ApplicationUser currentUser = OnLineUsers.GetSessionUser();
            DB.follow.FriendAccepted(id, currentUser.Id);
            return null;
        }
        //public ActionResult SendFriendshipRequest(string id)
        //{
        //    ApplicationUser currentUser = OnLineUsers.GetSessionUser();
        //    DB.FollowRepositories.Following(currentUser.Id, id);
        //    return null;
        //}
        //public ActionResult RemoveFriendship(string id)
        //{
        //    ApplicationUser currentUser = OnLineUsers.GetSessionUser();
        //    DB.FollowRepositories.Unfollow(currentUser.Id, id);
        //    return null;
        //}
        //public ActionResult AcceptFriendshipRequest(string id)
        //{
        //    ApplicationUser currentUser = OnLineUsers.GetSessionUser();
        //    DB.FollowRepositories.FriendAccepted(id, currentUser.Id);
        //    return null;
        //}
       
    }

}