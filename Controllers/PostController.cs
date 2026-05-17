using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FoodCo.Models;
using Microsoft.AspNet.Identity;

namespace FoodCo.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private readonly UserRepository _userRepository;
        private readonly PostRepository _postRepository;

        public PostController()
        {
            _userRepository = new UserRepository(new ApplicationDbContext());
            _postRepository = new PostRepository(new ApplicationDbContext());
        }

        // GET: Post
        public ActionResult Index()
        {
            return View(db.Posts.ToList());
        }

        // GET: Post/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts
                .Include(r => r.User)
                .Include(r => r.Reposts)
                .Include(r => r.Likes)
                .FirstOrDefault(r => r.Id == id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }


        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create()
        {
            try
            {
                var text = Request.Form["Text"];

                if (string.IsNullOrWhiteSpace(text))
                {
                    return Json(new { success = false, errors = new List<string> { "Text cannot be empty." } });
                }

                var file = Request.Files["Content"];
                byte[] fileContent = null;

                if (file != null && file.ContentLength > 0)
                {
                    if (file.ContentLength > 10 * 1024 * 1024) //10 MB
                    {
                        return Json(new { success = false, errors = new List<string> { "File size exceeds the 10 MB limit." } });
                    }

                    using (var binaryReader = new BinaryReader(file.InputStream))
                    {
                        fileContent = binaryReader.ReadBytes(file.ContentLength);
                    }
                }
   
                var post = new Post
                {
                    Text = text,
                    Content = fileContent,
                    UserId = User.Identity.GetUserId() 
                };

                if (ModelState.IsValid)
                {
                    db.Posts.Add(post);
                    await db.SaveChangesAsync(); 
                    return Json(new { success = true, message = "An unexpected error occurred. Please try again later." });
                }
                else
                {
            
                    return Json(new { success = false });
                }
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = "An unexpected error occurred. Please try again later." });
            }
        }


        // GET: Post/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    ApplicationDbContext db = new ApplicationDbContext();
        //    var admin = db.Admin;
        //    var isAdmin = false;

        //    foreach (var userAdmin in admin)
        //    {
        //        if (userAdmin.UserId == User.Identity.GetUserId() && userAdmin.IsAdmin == true)
        //        {
        //            isAdmin = true;
        //        }
        //    }

        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Post post = db.Posts
        //        .Include(p => p.User)
        //        .Include(p => p.Reposts)
        //        .Include(p => p.Likes)
        //        .FirstOrDefault(p => p.Id == id);

        //    if (post == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    string currentUserId = User.Identity.GetUserId();

        //    if (post.UserId != currentUserId && !isAdmin)
        //    {
        //        return PartialView("~/Views/Shared/ErrorPartials/_Forbidden.cshtml");
        //    }
        //    ViewBag.UserId = new SelectList(db.Users, "Id", "Name", post.UserId);

        //    return View(post);
        //}

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public JsonResult Edit([Bind(Include = "Id,Text,Content,Date,IsModified,UserId")] Post post)
        //{
        //    ApplicationDbContext db = new ApplicationDbContext();
        //    var admin = db.Admin;
        //    var isAdmin = false;

        //    foreach (var userAdmin in admin)
        //    {
        //        if (userAdmin.UserId == User.Identity.GetUserId() && userAdmin.IsAdmin == true)
        //        {
        //            isAdmin = true;
        //        }
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        string currentUserId = User.Identity.GetUserId();
        //        post.UserId = currentUserId;
        //        db.Entry(post).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return Json(new { success = true, redirectUrl = Url.Action("Index", "Feed") });
        //    }
        //    return Json(new { success = false, message = "Invalis model state" });
        //}

        // GET: Recipes/Edit/5
        public ActionResult Edit(int? id)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var admin = db.Admin;
            var isAdmin = false;

            foreach (var userAdmin in admin)
            {
                if (userAdmin.UserId == User.Identity.GetUserId() && userAdmin.IsAdmin == true)
                {
                    isAdmin = true;
                }
            }

            if (id == null)
            {
                return PartialView("~/Views/Shared/ErrorPartials/_BadRequest.cshtml");
            }

            Post post = db.Posts
                .Include(r => r.User)
                .Include(r => r.Reposts)
                .Include(r => r.Likes)
                .FirstOrDefault(r => r.Id == id);

            if (post == null)
            {
                return PartialView("~/Views/Shared/ErrorPartials/_PageNotFound.cshtml");
            }

            string currentUserId = User.Identity.GetUserId();

            if (post.UserId != currentUserId && !isAdmin)
            {
                return PartialView("~/Views/Shared/ErrorPartials/_Forbidden.cshtml");
            }

            ViewBag.UserId = new SelectList(db.Users, "Id", "Name", post.UserId);
            return View(post);
        }



        // POST: Recipes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Post post)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var admin = db.Admin;
            var isAdmin = false;

            foreach (var userAdmin in admin)
            {
                if (userAdmin.UserId == User.Identity.GetUserId() && userAdmin.IsAdmin == true)
                {
                    isAdmin = true;
                }
            }

            if (ModelState.IsValid)
            {
                Post oldPost = await db.Posts
                    .Include(r => r.User)
                    .Include(r => r.Reposts)
                    .Include(r => r.Likes)
                    .FirstOrDefaultAsync(r => r.Id == post.Id);
                if (oldPost == null)
                {
                    return View(post);
                }

                if (oldPost.UserId != User.Identity.GetUserId() && !isAdmin)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }


                oldPost.UserId = oldPost.UserId;
                oldPost.Text = post.Text;
                oldPost.IsModified = true;

                //if (ImageFile != null && ImageFile.ContentLength > 0)
                //{

                //    var validFileTypes = new List<string>
                //    {
                //         "image/jpeg", "image/png", "image/gif",
                //         "video/mp4", "video/avi", "video/quicktime",
                //         "video/webm"
                //    };


                //    if (!validFileTypes.Contains(ImageFile.ContentType))
                //    {
                //        ModelState.AddModelError("ImageFile", "Le fichier doit être une image (jpg, png, gif) ou une vidéo (mp4, avi, mov, webm).");
                //        ViewBag.UserId = new SelectList(db.Users, "Id", "Name", recipe.UserId);
                //        return View(recipe);
                //    }

                //    if (!string.IsNullOrEmpty(oldRecipe.ContentPath))
                //    {
                //        string oldFilePath = Server.MapPath(oldRecipe.ContentPath);
                //        if (System.IO.File.Exists(oldFilePath))
                //        {
                //            System.IO.File.Delete(oldFilePath);
                //        }
                //    }

                //string userFolder = oldPost.UserId;
                //string userFolderPath = Server.MapPath($"/Content_Data/Users/{userFolder}");

                //if (!Directory.Exists(userFolderPath))
                //{
                //    Directory.CreateDirectory(userFolderPath);
                //}

                //string fileName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                //string fileExtension = Path.GetExtension(ImageFile.FileName);
                //string uniqueFileName = $"{fileName}_{DateTime.Now.Ticks}{fileExtension}";

                //string filePath = Path.Combine(userFolderPath, uniqueFileName);
                //ImageFile.SaveAs(filePath);

                //oldRecipe.ContentPath = $"/Content_Data/Users/{userFolder}/{uniqueFileName}";
            //}

                db.Posts.AddOrUpdate(oldPost);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "Feed");
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "Name", post.UserId);
            return View(post);
        }
        // GET: Post/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Post post = db.Posts.Find(id);
            db.Posts.Remove(post);
            db.SaveChanges();
            return RedirectToAction("Index", "Feed");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public async Task<JsonResult> ToggleLike(string postId)
        {
            var result = await _postRepository.ToggleLike(int.Parse(postId), User.Identity.GetUserId());
            var updatedLikesCount = await _postRepository.GetLikesCount(int.Parse(postId));

            return Json(new { success = result, likesCount = updatedLikesCount });
        }

        [HttpPost]
        public async Task<JsonResult> AddComment(string postId, string comment)
        {
            var result = await _postRepository.AddComment(int.Parse(postId), User.Identity.GetUserId(), comment);

            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<JsonResult> AnswerComment(string postCommentId, string comment)
        {
            var result = await _postRepository.AddAnswer(int.Parse(postCommentId), User.Identity.GetUserId(), comment);

            return Json(new { success = result });
        }

        public ActionResult _PartialComments(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            List<PostComment> comments = new List<PostComment>();
            var commentsDB = db.PostComments.Include(r => r.Comments)
                .ToList()
                ;
            foreach (var comment in commentsDB)
            {
                if (comment.PostId == id)
                {
                    comment.User = db.Users.Find(comment.UserId);
                    comments.Add(comment);
                }
            }
            return PartialView(comments);
        }
        //public ActionResult _PartialAnswers(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    PostComment recipe = db.PostComments.Find(id);
        //    if (recipe == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    List<AnswerComments> Answers = new List<AnswerComments>();
        //    var answerDB = db.AnswerComments.ToList();
        //    foreach (var answer in answerDB)
        //    {
        //        if (answer.commentId == id)
        //        {
        //            answer.User = db.Users.Find(answer.UserId);
        //            Answers.Add(answer);
        //        }
        //    }
        //    return PartialView(Answers);
        //}

    }
}
