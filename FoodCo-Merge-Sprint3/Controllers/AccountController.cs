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
using FoodCo.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using FoodCo.Migrations;
using System.Data.Entity.Migrations;

namespace FoodCo.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly UserRepository _userRepository;

        private ApplicationDbContext DB = new ApplicationDbContext();

        public AccountController()
        {
            _userRepository = new UserRepository(new ApplicationDbContext());
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: /{username}
        [Authorize]
        public async Task<ActionResult> Profile(string username)
        {
            var user = await _userRepository.GetUserByUsername(username);
            var curentUserId = User.Identity.GetUserId(); 
            if (user == null)
            {
                return HttpNotFound();
            }
            if(DB.Blocks.Where(b=>b.BlockerId == user.Id && b.BlockedId == curentUserId && b.BlockStatus==EnumBlockStatus.Blocked).Any())
            {
                return HttpNotFound();
            }

            return View(user);
        }

        //public FileResult Photo()
        //{
        //    var db = HttpContext.GetOwinContext().Get<ApplicationDbContext>();

        //    var userId = User.Identity.GetUserId();
        //    var user = db.Users.Where(x => x.Id == userId).FirstOrDefault();


        //    if (user.ProfilePicture != null)
        //    {
        //        return new FileContentResult(user.ProfilePicture, "image/jpeg");
        //    }
        //    else
        //    {
        //        return new FilePathResult("/Content/account/images/default_pfp.jpg", "image/jpeg");
        //    }
        //}

        public string GetUsernameById(string id)
        {
            var db = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            var user = db.Users.Where(x => x.Id == id).FirstOrDefault();

            if (user != null)
            {

                return user.UserName;
            }

            return "";
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            //if (User.Identity.IsAuthenticated)
            //{
            //    return RedirectToAction("Index", "Feed");
            //}

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Username))
            {
                ModelState.AddModelError("Username", "Username est requis.");
                return View(model);
            }

            if (_userRepository.IsEmailVerified(_userRepository.GetUserIdByUsername(model.Username)))
            {
                var result = await SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, shouldLockout: false);
                switch (result)
                {
                    case SignInStatus.Success:
                        
                        Session["currentUserId"] = _userRepository.GetUserIdByUsername(model.Username);
                        Session["isLoggedIn"] = true;
                        OnLineUsers.AddSessionUser(_userRepository.GetUserIdByUsername(model.Username));
                        bool isOnline = OnLineUsers.IsOnLine(_userRepository.GetUserIdByUsername(model.Username));

                        ApplicationUser applicationUser = OnLineUsers.GetSessionUser();
                        if (applicationUser.IsAccountBlocked)
                        {
                            ModelState.AddModelError("", $"Votre compte est bloquer du site");
                            return View(model);
                        }
                        return RedirectToLocal(returnUrl);
                    case SignInStatus.LockedOut:
                        return View("Lockout");
                    case SignInStatus.RequiresVerification:
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                    case SignInStatus.Failure:
                    default:
                        ModelState.AddModelError("", $"Username ou mot de passe est incorrect.");
                        return View(model);
                }
                
            }
            ModelState.AddModelError("", $"Veuillez vérifier votre compte.");
            return View(model);
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    Session["isLoggedIn"] = true;
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Feed");
            }

            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                string defaultPfpPath = "/Content/account/images/default_pfp.jpg";

                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Name = model.Name,
                    Lastname =
                    model.Lastname,
                    Email = model.Email,
                    ProfilePicturePath = defaultPfpPath
                };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    //await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    var fullname = user.Name + " " + user.Lastname;
                    string Subject = "Vérification de compte";

                    SMTP.SendEmail(fullname, user.Email, Subject, SMTP.EmailVerificationTemplate(callbackUrl));


                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {

            if (ModelState.IsValid)
            {
                var user = await _userRepository.GetUserByEmail(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    TempData["ConfirmationForgotEmail"] = model.Email;
                    return View("ForgotPasswordConfirmation");
                }

                //For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                //Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                var fullname = user.Name + " " + user.Lastname;
                string Subject = "Réinitialisation de Mot de Passe";

                SMTP.SendEmail(fullname, user.Email, Subject, SMTP.ForgotPasswordTemplate(callbackUrl));

                //await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                TempData["ConfirmationForgotEmail"] = model.Email;
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            ViewBag.ConfirmationForgotEmail = TempData["ConfirmationForgotEmail"];
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userRepository.GetUserByEmail(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("Login", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("Login", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Session.Clear();
            Session.Abandon();

            // Clear the cache
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            OnLineUsers.RemoveSessionUser();

            return RedirectToAction("Index", "Home");
        }


        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        [Authorize]
        public ActionResult Notifications()
        {
            return View();
        }
        public async Task<JsonResult> UserHasNotifications()
        {
            var username = await _userRepository.GetUsernameAsync(User.Identity.GetUserId());

            var userId = User.Identity.GetUserId();
            var mentionsPost = await DB.PostComments
                .Where(c => c.isMention && c.Text.ToLower().Contains("@" + username.ToLower()) && c.UserId != userId)
                .ToListAsync();
            var mentionsRecipe = await DB.RecipeComments
                .Where(c => c.isMention && c.Text.ToLower().Contains("@" + username.ToLower()) && c.UserId != userId)
                .ToListAsync();
            //var unblockedMentions = new List<PostComment>();
            //foreach(var mention in mentions)
            //{
            //    if (!DB.Blocks.Where(b => b.BlockerId == mention.Post.UserId && b.BlockedId == userId && b.BlockStatus == EnumBlockStatus.Blocked).Any() &&
            //            !DB.Blocks.Where(b => b.BlockedId == mention.Post.UserId && b.BlockerId == userId && b.BlockStatus == EnumBlockStatus.Blocked).Any())
            //        unblockedMentions.Add(mention);
            //}
            var messages = await DB.Chats
                .Where(m => m.ResponderId == userId && !m.IsRead)
                .ToListAsync();
            var unblockedMessages = new List<Chat>();
            foreach(var message in messages)
            {
                if (!DB.Blocks.Where(b => b.BlockerId == message.ResponderId && b.BlockedId == message.UserId && b.BlockStatus == EnumBlockStatus.Blocked).Any() &&
                        !DB.Blocks.Where(b => b.BlockedId == message.ResponderId && b.BlockerId == message.UserId && b.BlockStatus == EnumBlockStatus.Blocked).Any() &&
                        (!DB.Follows.Where(f => f.FollowerId == message.UserId && f.FollowedUserId == message.ResponderId && f.FriendshipStatus == EnumFriendshipStatus.UnFollow).Any() ||
                        !DB.Follows.Where(f => f.FollowedUserId == message.UserId && f.FollowerId == message.ResponderId && f.FriendshipStatus == EnumFriendshipStatus.UnFollow).Any()))
                    unblockedMessages.Add(message);
            }

            if (unblockedMessages.Any() || mentionsPost.Any() ||mentionsRecipe.Any())
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }


        [Authorize]
        public async Task<ActionResult> _PartialNotifications()
        {
            var frenchDates = new System.Globalization.CultureInfo("fr-FR");
            var userId = User.Identity.GetUserId();
            var listMsg = new List<NotificationViewModel>();


            var msgNotRead = await DB.Chats
                .Where(m => m.ResponderId == userId && !m.IsRead)
                .OrderByDescending(m => m.Date)
                .ToListAsync();


            foreach (var msg in msgNotRead)
            {
                var user = await _userRepository.GetAsync(msg.UserId);
                if (user != null)
                {
                    string senderUsername = user.UserName;
                    string senderProfilePicture = user.ProfilePicturePath;
                    string senderName = user.Name;

                    var timeSpan = DateTime.Now - msg.Date;

                    string relativeTime;
                    if (timeSpan.TotalMinutes < 60)
                    {
                        relativeTime = $"il y a {(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes > 1 ? "s" : "")}";
                    }
                    else if (timeSpan.TotalHours < 24)
                    {
                        relativeTime = $"il y a {(int)timeSpan.TotalHours} heure{(timeSpan.TotalHours > 1 ? "s" : "")}";
                    }
                    else if (timeSpan.TotalDays < 7)
                    {
                        relativeTime = $"il y a {(int)timeSpan.TotalDays} jour{(timeSpan.TotalDays > 1 ? "s" : "")}";
                    }
                    else
                    {
                        relativeTime = msg.Date.ToString("MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    if(!DB.Blocks.Where(b => b.BlockerId == user.Id && b.BlockedId == userId && b.BlockStatus == EnumBlockStatus.Blocked).Any() &&
                        !DB.Blocks.Where(b => b.BlockedId == user.Id && b.BlockerId == userId && b.BlockStatus == EnumBlockStatus.Blocked).Any() &&
                        (!DB.Follows.Where(f => f.FollowerId == msg.UserId && f.FollowedUserId == msg.ResponderId && f.FriendshipStatus == EnumFriendshipStatus.UnFollow).Any() ||
                        !DB.Follows.Where(f => f.FollowedUserId == msg.UserId && f.FollowerId == msg.ResponderId && f.FriendshipStatus == EnumFriendshipStatus.UnFollow).Any()))
                        listMsg.Add(new NotificationViewModel
                        {
                            Sender = senderUsername,
                            SenderProfilePicture = senderProfilePicture,
                            SenderName = senderName,
                            Date = relativeTime,
                            Type = "chat"
                        });

                }
            }

            var username = await _userRepository.GetUsernameAsync(User.Identity.GetUserId());


            var mentionsPost = await DB.PostComments
                .Where(c => c.isMention && c.Text.ToLower().Contains("@" + username.ToLower()))
                .ToListAsync();
            var mentionsRecipe = await DB.RecipeComments
               .Where(c => c.isMention && c.Text.ToLower().Contains("@" + username.ToLower()))
               .ToListAsync();

            foreach (var mention in mentionsPost)
            {
                var user = await _userRepository.GetAsync(mention.UserId);
                if (user != null)
                {
                    if (user.Id != User.Identity.GetUserId())
                    {


                        string senderUsername = user.UserName;
                        string senderProfilePicture = user.ProfilePicturePath;
                        string senderName = user.Name;

                        var timeSpan = DateTime.Now - mention.Date;

                        string relativeTime;
                        if (timeSpan.TotalMinutes < 60)
                        {
                            relativeTime = $"il y a {(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes > 1 ? "s" : "")}";
                        }
                        else if (timeSpan.TotalHours < 24)
                        {
                            relativeTime = $"il y a {(int)timeSpan.TotalHours} heure{(timeSpan.TotalHours > 1 ? "s" : "")}";
                        }
                        else if (timeSpan.TotalDays < 7)
                        {
                            relativeTime = $"il y a {(int)timeSpan.TotalDays} jour{(timeSpan.TotalDays > 1 ? "s" : "")}";
                        }
                        else
                        {
                            relativeTime = mention.Date.ToString("MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                        }
                        
                            listMsg.Add(new NotificationViewModel
                        {
                            Sender = senderUsername,
                            SenderProfilePicture = senderProfilePicture,
                            SenderName = senderName,
                            Date = relativeTime,
                            Type = "mention"
                        });

                        bool changesMade = false;

                        foreach (var m in mentionsPost.Where(m => m.isMention))
                        {
                            if (m.isMention)
                            {
                                m.isMention = false;
                                changesMade = true;
                            }
                        }

                        if (changesMade)
                        {
                            await DB.SaveChangesAsync();
                        }
                    }
                }
            }
            foreach (var mention in mentionsRecipe)
            {
                var user = await _userRepository.GetAsync(mention.UserId);
                if (user != null)
                {
                    if (user.Id != User.Identity.GetUserId())
                    {


                        string senderUsername = user.UserName;
                        string senderProfilePicture = user.ProfilePicturePath;
                        string senderName = user.Name;

                        var timeSpan = DateTime.Now - mention.Date;

                        string relativeTime;
                        if (timeSpan.TotalMinutes < 60)
                        {
                            relativeTime = $"il y a {(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes > 1 ? "s" : "")}";
                        }
                        else if (timeSpan.TotalHours < 24)
                        {
                            relativeTime = $"il y a {(int)timeSpan.TotalHours} heure{(timeSpan.TotalHours > 1 ? "s" : "")}";
                        }
                        else if (timeSpan.TotalDays < 7)
                        {
                            relativeTime = $"il y a {(int)timeSpan.TotalDays} jour{(timeSpan.TotalDays > 1 ? "s" : "")}";
                        }
                        else
                        {
                            relativeTime = mention.Date.ToString("MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                        }

                        listMsg.Add(new NotificationViewModel
                        {
                            Sender = senderUsername,
                            SenderProfilePicture = senderProfilePicture,
                            SenderName = senderName,
                            Date = relativeTime,
                            Type = "mention"
                        });

                        bool changesMade = false;

                        foreach (var m in mentionsRecipe.Where(m => m.isMention))
                        {
                            if (m.isMention)
                            {
                                m.isMention = false;
                                changesMade = true;
                            }
                        }

                        if (changesMade)
                        {
                            await DB.SaveChangesAsync();
                        }
                    }
                }
            }
            return View(listMsg);
        }

        [Authorize]
        public async void ClearNotification()
        {
            var username = await _userRepository.GetUsernameAsync(User.Identity.GetUserId());


            var mentions = await DB.PostComments
                .Where(c => c.isMention && c.Text.ToLower().Contains("@" + username.ToLower()))
                .ToListAsync();

            foreach (var mention in mentions)
            {
                mention.isMention = false;
            }

            await DB.SaveChangesAsync();
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

                var userData = DB.Users
                    .Where(u => u.UserName.Contains(query))
                    .Select(u => new
                    {
                        Username = u.UserName,
                        ProfilePicture = u.ProfilePicturePath
                    })
                    .ToList();

                var currentUserId = User.Identity.GetUserId();
                var usersData = userData;
                foreach (var data in userData)
                {
                    
                    var userId = _userRepository.GetUserIdByUsername(data.Username);
                    if (DB.Blocks.Where(b => b.BlockerId == userId && b.BlockedId == currentUserId && b.BlockStatus == EnumBlockStatus.Blocked).Any())
                    {
                        usersData.Remove(data);
                    }
                }
                return Json(usersData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = "An unexpected error occurred. Please try again later." }, JsonRequestBehavior.AllowGet);
            }
        }

        





        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}