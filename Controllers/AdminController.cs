using FoodCo.Models;
using Mail;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FoodCo.Controllers
{
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private readonly UserRepository _userRepository;
        private readonly PostRepository _postRepository;
        private readonly RecipeRepository _recipeRepository;
        private readonly AdminRepository _adminRepository;
        public AdminController()
        {
            _userRepository = new UserRepository(new ApplicationDbContext());
            _postRepository = new PostRepository(new ApplicationDbContext());
            _adminRepository = new AdminRepository(new ApplicationDbContext());
            _recipeRepository = new RecipeRepository(new ApplicationDbContext());
        }

        // GET: Admin
        public ActionResult Index()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var _userId = Session["currentUserId"]?.ToString();
            var admin = db.Admin;

            foreach (var userAdmin in admin)
            {
                if (userAdmin.UserId == _userId && userAdmin.IsAdmin == false)
                {
                    return RedirectToAction("Login", "Account");
                }
                else if(userAdmin.UserId == _userId && userAdmin.IsAdmin == true)
                {
                    return View();
                }
            }
            return RedirectToAction("Login", "Account");

        }
        [HttpPost]
        public async Task<JsonResult> BlockWebSiteUser(string id)
        {
            //ApplicationUser currentUser = OnLineUsers.GetSessionUser();
            ApplicationUser UserToBlock = await _userRepository.GetAsync(id);

            var fullname = UserToBlock.Name + " " + UserToBlock.Lastname;
            string Subject = "Votre compte est bloquer";

            if (UserToBlock != null)
            {
                UserToBlock.IsAccountBlocked = true;


                db.Users.AddOrUpdate(UserToBlock);
                await db.SaveChangesAsync();
                SMTP.SendEmail(fullname, UserToBlock.Email, Subject, SMTP.UserIsBlock());
                return Json( new { success = true}, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            //UserToBlock.Id = UserToBlock.Id;
            //UserToBlock.Name = UserToBlock.Name;
            //UserToBlock.Lastname = UserToBlock.Lastname;
            //UserToBlock.CreationDate = UserToBlock.CreationDate;
            //UserToBlock.ProfilePicturePath = UserToBlock.ProfilePicturePath;
            //UserToBlock.Biography = UserToBlock.Biography;
            //UserToBlock.HasNotification = UserToBlock.HasNotification;
        }
        [HttpPost]
        public async Task<JsonResult> UnBlockWebSiteUser(string id)
        {
            //ApplicationUser currentUser = OnLineUsers.GetSessionUser();
            ApplicationUser UserToBlock = await _userRepository.GetAsync(id);

            var fullname = UserToBlock.Name + " " + UserToBlock.Lastname;
            string Subject = "Votre compte est débloquer";

            if (UserToBlock != null)
            {
                UserToBlock.IsAccountBlocked = false;


                db.Users.AddOrUpdate(UserToBlock);
                await db.SaveChangesAsync();
                SMTP.SendEmail(fullname, UserToBlock.Email, Subject, SMTP.UserIsUnblock());
                return Json( new { success = true}, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            //UserToBlock.Id = UserToBlock.Id;
            //UserToBlock.Name = UserToBlock.Name;
            //UserToBlock.Lastname = UserToBlock.Lastname;
            //UserToBlock.CreationDate = UserToBlock.CreationDate;
            //UserToBlock.ProfilePicturePath = UserToBlock.ProfilePicturePath;
            //UserToBlock.Biography = UserToBlock.Biography;
            //UserToBlock.HasNotification = UserToBlock.HasNotification;
        }
        [HttpPost]
        public async Task<JsonResult> DeleteUser(string id)
        {
            var adminUserId = User.Identity.GetUserId();
            var admin = db.Admin.Any(a => a.UserId == adminUserId);
            var UserToDelete = await db.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (UserToDelete == null || !admin)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            var userListPostComment = await db.PostComments.Where(pc => pc.UserId == id).ToListAsync();
            var userListRecipeComment = await db.RecipeComments.Where(pc => pc.UserId == id).ToListAsync();
            var userListRecipeLike = await db.RecipeLikes.Where(pc => pc.UserId == id).ToListAsync();
            var userListPostLike = await db.PostLikes.Where(pc => pc.UserId == id).ToListAsync();
            var userListRecipeReviews = await db.RecipeReviews.Where(pc => pc.UserId == id).ToListAsync();
            var userListBlocked = await db.Blocks.Where(pc => pc.BlockedId == id).ToListAsync();
            var userListBlocker = await db.Blocks.Where(pc => pc.BlockerId == id).ToListAsync();
            var userListChat = await db.Chats.Where(pc => pc.UserId == id).ToListAsync();
            var userListbookmarks = await db.RecipeBooks.Where(pc => pc.UserId == id).ToListAsync();
            var userListFollowed = await db.Follows.Where(pc => pc.FollowedUserId == id).ToListAsync();
            var userListFollowing = await db.Follows.Where(pc => pc.FollowerId == id).ToListAsync();


            if (userListPostComment != null)
            {
                foreach (var postComment in userListPostComment)
                {
                    db.PostComments.Remove(postComment);
                }
            }
            if (userListFollowed != null)
            {
                foreach (var followed in userListFollowed)
                {
                    db.Follows.Remove(followed);
                }
            }
            if (userListFollowing != null)
            {
                foreach (var follower in userListFollowing)
                {
                    db.Follows.Remove(follower);
                }
            }
            if (userListChat != null)
            {
                foreach (var chat in userListChat)
                {
                    db.Chats.Remove(chat);
                }
            }
            if (userListbookmarks != null)
            {
                foreach (var bookmarks in userListbookmarks)
                {
                    db.RecipeBooks.Remove(bookmarks);
                }
            }
            if (userListBlocked != null)
            {
                foreach (var block in userListBlocked)
                {
                    db.Blocks.Remove(block);
                }
            }
            if (userListBlocker != null)
            {
                foreach (var block in userListBlocker)
                {
                    db.Blocks.Remove(block);
                }
            }
            if (userListRecipeReviews != null)
            {
                foreach (var recipeReview in userListRecipeReviews)
                {
                    db.RecipeReviews.Remove(recipeReview);
                }
            }
            if (userListRecipeLike != null)
            {
                foreach (var postLike in userListPostLike)
                {
                    db.PostLikes.Remove(postLike);
                }
            }
            if (userListRecipeLike != null)
            {
                foreach (var recipeLike in userListRecipeLike)
                {
                    db.RecipeLikes.Remove(recipeLike);
                }
            }

            if (userListRecipeComment != null)
            {
                foreach (var recipeComment in userListRecipeComment)
                {
                    db.RecipeComments.Remove(recipeComment);

                }
            }


            var fullname = UserToDelete.Name + " " + UserToDelete.Lastname;
            string Subject = "Votre compte est supprimer";

            db.Users.Remove(UserToDelete);
            await db.SaveChangesAsync();

            SMTP.SendEmail(fullname, UserToDelete.Email, Subject, SMTP.UserIsDelete());
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult SearchUsers(string query)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Json(new { JsonRequestBehavior.AllowGet });
                }

                var userData = db.Users
                    .Where(u => u.UserName.Contains(query))
                    .Select(u => new
                    {
                        Username = u.UserName,
                        ProfilePicture = u.ProfilePicturePath,
                        isAccountBlock = u.IsAccountBlocked,
                        userId = u.Id
                    })
                    .ToList();

                var currentUserId = User.Identity.GetUserId();
                var usersData = userData;
                foreach (var data in userData)
                {

                    var userId = _userRepository.GetUserIdByUsername(data.Username);
                    
                }
                return Json(usersData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = "An unexpected error occurred. Please try again later." }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}