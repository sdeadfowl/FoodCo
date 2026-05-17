using FoodCo.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Net;

namespace FoodCo.Controllers
{
    [Authorize]
    public class FeedController : Controller
    {
        private PostRepository postRepo;

        private ApplicationDbContext DB = new ApplicationDbContext();
        // GET: Feed
        public ActionResult Index()
        {
            if (Session["isLoggedIn"] == null || !(bool)Session["isLoggedIn"])
            {
                return RedirectToAction("Login", "Account");
            }

            //var db = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            //var users = await db.Users.ToListAsync();
            //var allPosts = new List<Post>();

            //foreach (var user in users)
            //{
            //    var userId = user.Id;
            //    var posts = await db.Posts.Where(p=>p.UserId.ToString() == user.Id.ToString()).ToListAsync();

            //    allPosts.AddRange(posts);
            //}

            return View();
        }

        //[Authorize] 
        //public async Task<ActionResult> _PartialFeed()
        //{
        //    try
        //    {
        //        using (var db = HttpContext.GetOwinContext().Get<ApplicationDbContext>())
        //        {

        //            var allPosts = await db.Posts
        //                                   .Include(p => p.User)
        //                                   .Include(p => p.Comments)
        //                                   .Include(p => p.Likes)
        //                                   .Include(p => p.Reposts)
        //                                   //.Select(p => new
        //                                   //{
        //                                   //    Post = p,
        //                                   //    CommentCount = p.Comments.Count,
        //                                   //    LikeCount = p.Likes.Count,
        //                                   //    RepostCount = p.Reposts.Count
        //                                   //})
        //                                   .ToListAsync();


        //            return PartialView("_PartialFeed", allPosts);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An error occurred while loading th feed.");
        //    }
        //}
        //[Authorize]
        //public async Task<ActionResult> _PartialFeed(int page = 1, int pageSize = 10)
        //{
        //    try
        //    {
        //        using (var db = HttpContext.GetOwinContext().Get<ApplicationDbContext>())
        //        {
        //            var posts = await db.Posts
        //                .Include(p => p.User)
        //                .Include(p => p.Comments)
        //                .Include(p => p.Likes)
        //                .Include(p => p.Reposts)
        //                .OrderByDescending(p => p.Date)
        //                .Skip((page - 1) * pageSize)
        //                .Take(pageSize)
        //                .ToListAsync();


        //            // Check if posts are null or empty
        //            if (posts == null || !posts.Any())
        //            {
        //                return PartialView("_PartialFeed", new List<Post>());
        //            }

        //            return PartialView("_PartialFeed", posts);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception if needed
        //        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An error occurred while loading the feed.");
        //    }
        //}

        [Authorize]
        public async Task<ActionResult> _PartialFeed(int page = 1, int pageSize = 10)
        {
            try
            {
                using (var db = HttpContext.GetOwinContext().Get<ApplicationDbContext>())
                {
                    var userId = User.Identity.GetUserId(); // Récupère l'ID de l'utilisateur connecté

                    // Récupérer les recettesbook
                    var userRecipeIds = await db.RecipeBooks
                        .Where(rb => rb.UserId == userId)
                        .Select(rb => rb.RecipeId)
                        .ToListAsync();

                    // Récupérer les posts
                    var posts = await db.Posts
                        .Include(p => p.User)
                        .Include(p => p.Comments)
                        .Include(p => p.Likes)
                        .Include(p => p.Reposts)
                        .OrderByDescending(p => p.Date)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    // Récupérer les recettes
                    var recipes = await db.Recipes
                        .Include(r => r.User)
                        .Include(r => r.Comments)
                        .Include(r => r.Likes)
                        .Include(r => r.Reposts)
                        .Include(r => r.Reviews)
                        .OrderByDescending(r => r.Date)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    // Combine posts and recipes
                    var feedItems = new List<object>();
                    feedItems.AddRange(posts);
                    feedItems.AddRange(recipes);

                    // Order combined list by date
                    feedItems = feedItems
                        .OrderByDescending(item =>
                            item is Post ? ((Post)item).Date : ((Recipe)item).Date)
                        .ToList();
                    var feed = new List<object>();
                    foreach (var item in feedItems)
                    {
                        if (item is Post)
                        {
                            if (!DB.Blocks.Where(b => b.BlockerId == ((Post)item).UserId && b.BlockedId == userId && b.BlockStatus == EnumBlockStatus.Blocked).Any() &&
                                !DB.Blocks.Where(b => b.BlockedId == ((Post)item).UserId && b.BlockerId == userId && b.BlockStatus == EnumBlockStatus.Blocked).Any() &&
                                DB.Users.Where(u=> u.Id == ((Post)item).UserId && u.IsPrivate == false).Any())
                            {
                                feed.Add(item);
                            }
                        }
                        else
                        {
                            if (!DB.Blocks.Where(b => b.BlockerId == ((Recipe)item).UserId && b.BlockedId == userId && b.BlockStatus == EnumBlockStatus.Blocked).Any() &&
                                !DB.Blocks.Where(b => b.BlockedId == ((Recipe)item).UserId && b.BlockerId == userId && b.BlockStatus == EnumBlockStatus.Blocked).Any() &&
                                DB.Users.Where(u => u.Id == ((Recipe)item).UserId && u.IsPrivate == false).Any())
                            {
                                feed.Add(item);
                            }
                        }
                    }

                    // if feed is empty
                    if (!feed.Any())
                    {
                        return PartialView("_PartialFeed", new List<object>());
                    }

                    

                    // Passer les ids des recettes à la vue pour permettre l'ajout/retrait
                    ViewBag.UserRecipeIds = userRecipeIds;

                    return PartialView("_PartialFeed", feed);
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An error occurred while loading the feed.");
            }
        }

        public async Task<ActionResult> _PartialFeedProfile(string UserId, int page = 1, int pageSize = 10)
        {
            try
            {
                using (var db = HttpContext.GetOwinContext().Get<ApplicationDbContext>())
                {
                    //posts
                    var posts = await db.Posts
                        .Include(p => p.User)
                        .Include(p => p.Comments)
                        .Include(p => p.Likes)
                        .Include(p => p.Reposts)
                        .OrderByDescending(p => p.Date)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    //recipes
                    var recipes = await db.Recipes
                        .Include(r => r.User)
                        .Include(r => r.Comments)
                        .Include(r => r.Likes)
                        .Include(r => r.Reposts)
                        .Include(r => r.Reviews)
                        .OrderByDescending(r => r.Date)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
                    var userPosts = posts.FindAll(p => p.UserId == UserId);
                    var userRecipes = recipes.FindAll(r => r.UserId == UserId);

                    
                    // Combine posts and recipes
                    var feedItems = new List<object>();
                    feedItems.AddRange(userPosts);
                    feedItems.AddRange(userRecipes);

                    // Order combined list by date
                    feedItems = feedItems
                        .OrderByDescending(item =>
                            item is Post ? ((Post)item).Date : ((Recipe)item).Date)
                        .ToList();

                    //if feed is empty
                    if (!feedItems.Any())
                    {
                        return PartialView("_PartialFeedProfile", new List<object>());
                    }

                    return PartialView("_PartialFeedProfile", feedItems);
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An error occurred while loading the feed.");
            }
        }
        public async Task<ActionResult> _PartialFeedLike(string UserId, int page = 1, int pageSize = 10)
        {

            try
            {
                using (var db = HttpContext.GetOwinContext().Get<ApplicationDbContext>())
                {
                    //posts
                    var posts = await db.Posts
                        .Include(p => p.User)
                        .Include(p => p.Comments)
                        .Include(p => p.Likes)
                        .Include(p => p.Reposts)
                        .OrderByDescending(p => p.Date)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    //recipes
                    var recipes = await db.Recipes
                        .Include(r => r.User)
                        .Include(r => r.Comments)
                        .Include(r => r.Likes)
                        .Include(r => r.Reposts)
                        .Include(r => r.Reviews)
                        .OrderByDescending(r => r.Date)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    List<PostLike> postLikes = new List<PostLike>();
                    List<RecipeLike> recipeLikes = new List<RecipeLike>();

                    //foreach (var post in posts)
                    //{
                    //    foreach(var like in post.Likes)
                    //    {
                    //        if(like.UserId == UserId)
                    //        {
                    //            postLikes.Add(like);
                    //        }
                    //    }
                    //}
                    //foreach (var recipe in recipes)
                    //{
                    //    foreach (var like in recipe.Likes)
                    //    {
                    //        if (like.UserId == UserId)
                    //        {
                    //            recipeLikes.Add(like);
                    //        }
                    //    }
                    //}

                    

                    var postLike = db.PostLikes.Where(pl => pl.UserId == UserId).ToList();
                    var recipeLike = db.RecipeLikes.Where(pl => pl.UserId == UserId).ToList();

                    var userPosts = new List<Post>();
                    var userRecipes = new List<Recipe>();

                    foreach (var pl in postLike)
                    {
                            Post post = db.Posts.Where(p => p.Id == pl.PostId).FirstOrDefault();
                        foreach (var user in DB.Users)
                        {
                            if (user.Id == post.UserId && !user.IsPrivate)
                            {
                                userPosts.Add(post);
                            }
                        }
                    }
                    foreach (var pl in recipeLike)
                    {
                        
                            Recipe recipe = db.Recipes.Where(p => p.Id == pl.RecipeId).FirstOrDefault();
                        foreach (var user in DB.Users) {
                            if (user.Id == recipe.UserId && !user.IsPrivate)
                            {
                                userRecipes.Add(recipe);
                            }
                        }
                    }

                    // Combine posts and recipes
                    var feedItems = new List<object>();
                    feedItems.AddRange(userPosts);
                    feedItems.AddRange(userRecipes);

                    // Order combined list by date
                    feedItems = feedItems
                        .OrderByDescending(item =>
                            item is Post ? ((Post)item).Date : ((Recipe)item).Date)
                        .ToList();

                    //if feed is empty
                    if (!feedItems.Any())
                    {
                        return PartialView("_PartialFeedLike", new List<object>());
                    }

                    return PartialView("_PartialFeedLike", feedItems);
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An error occurred while loading the feed.");
            }
        }

        [HttpGet]
        public async Task<ActionResult> _PartialFeedBook(string userId, int page = 1, int pageSize = 10)
        {

            try
            {
                using (var db = HttpContext.GetOwinContext().Get<ApplicationDbContext>())
                {
                    //string userId = User.Identity.GetUserId();
                    List<RecipeBook> recipeBook = await db.RecipeBooks.Where(rb => rb.UserId == userId).ToListAsync();

                    //var recipeLivre = db.RecipeBooks.Where(pl => pl.UserId == UserId).ToList();
                    List<Recipe> recipeBooks = new List<Recipe>();
                    if (recipeBook.Any())
                    {

                        foreach (var rl in recipeBook)
                        {
                            var _recipe = await db.Recipes
                                .Where(r => r.Id == rl.RecipeId)
                                .Include(r => r.User)
                                .Include(r => r.Comments)
                                .Include(r => r.Likes)
                                .Include(r => r.Reposts)
                                .Include(r => r.Reviews)
                                .FirstOrDefaultAsync();
                            recipeBooks.Add(_recipe);
                        }
                    }


                    if (recipeBook == null || !recipeBooks.Any())
                    {
                        return PartialView("_PartialFeedBook");
                    }

                    return PartialView("_PartialFeedBook", recipeBooks);
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An error occurred while loading the feed." + ex);
            }
        }

        // GET: Feed
        public ActionResult Explore()
        {
            if (Session["isLoggedIn"] == null || !(bool)Session["isLoggedIn"])
            {
                return RedirectToAction("Login", "Account");
            }


            return View();
        }
    }
}
