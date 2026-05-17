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
    public class RecipeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private readonly UserRepository _userRepository;
        private readonly RecipeRepository _recipeRepository;
        private readonly ApplicationDbContext _context;


        public RecipeController()
        {
            _userRepository = new UserRepository(new ApplicationDbContext());
            _recipeRepository = new RecipeRepository(new ApplicationDbContext());
            _context = new ApplicationDbContext();

        }

        // GET: Recipes
        public ActionResult Index()
        {
            var recipes = db.Recipes
                .Include(r => r.User);
            return View(recipes.ToList());
        }

        // GET: Recipes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();

            Recipe recipe = db.Recipes
                .Include(r => r.User)
                .Include(r => r.Reposts)
                .Include(r => r.Likes)
                .Include(r => r.Ingredients)
                .Include(r => r.RecipeSteps)
                .FirstOrDefault(r => r.Id == id);

            if (recipe == null)
            {
                return HttpNotFound();
            }
            var hasReviewed = db.RecipeReviews.Any(r => r.RecipeId == id && r.UserId == userId);

            ViewBag.HasReviewed = hasReviewed;

            return View(recipe);
        }


        // GET: Recipes/Create
        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(db.Users, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Recipe recipe, List<RecipeIngredient> Ingredients, List<RecipeStep> RecipeSteps, HttpPostedFileBase ImageFile, string Tag)
        {
            if (ModelState.IsValid)
            {
                recipe.Tag = Tag;
                recipe.Date = DateTime.Now;
                recipe.UserId = User.Identity.GetUserId();
                recipe.Ingredients = Ingredients;
                recipe.RecipeSteps = RecipeSteps;

                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    string userFolder = recipe.UserId;

                    string userFolderPath = Server.MapPath($"/Content_Data/Users/{userFolder}");
                    if (!Directory.Exists(userFolderPath))
                    {
                        Directory.CreateDirectory(userFolderPath);
                    }

                    string fileName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                    string fileExtension = Path.GetExtension(ImageFile.FileName);
                    string uniqueFileName = $"{fileName}_{DateTime.Now.Ticks}{fileExtension}";

                    string filePath = Path.Combine(userFolderPath, uniqueFileName);
                    ImageFile.SaveAs(filePath);

                    recipe.ContentPath = $"/Content_Data/Users/{userFolder}/{uniqueFileName}";
                }

                db.Recipes.Add(recipe);
                db.SaveChanges();

                return RedirectToAction("Index", "Feed");
            }

            ViewBag.UserId = new SelectList(db.Users, "Id", "Name", recipe.UserId);
            return View(recipe);
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(Recipe recipe, List<RecipeIngredient> Ingredients, List<RecipeStep> RecipeSteps)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        recipe.Date = DateTime.Now;
        //        recipe.UserId = User.Identity.GetUserId();

        //        // Assigning the ingredients and steps to the recipe
        //        recipe.Ingredients = Ingredients;
        //        recipe.RecipeSteps = RecipeSteps;

        //        // Saving to the database
        //        db.Recipes.Add(recipe);
        //        db.SaveChanges();

        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.UserId = new SelectList(db.Users, "Id", "Name", recipe.UserId);
        //    return View(recipe);
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

            Recipe recipe = db.Recipes
                .Include(r => r.User)
                .Include(r => r.Reposts)
                .Include(r => r.Likes)
                .Include(r => r.Ingredients)
                .Include(r => r.RecipeSteps)
                .FirstOrDefault(r => r.Id == id);

            if (recipe == null)
            {
                return PartialView("~/Views/Shared/ErrorPartials/_PageNotFound.cshtml");
            }

            string currentUserId = User.Identity.GetUserId();

            if (recipe.UserId != currentUserId && !isAdmin)
            {
                return PartialView("~/Views/Shared/ErrorPartials/_Forbidden.cshtml");
            }

            ViewBag.UserId = new SelectList(db.Users, "Id", "Name", recipe.UserId);
            return View(recipe);
        }



        // POST: Recipes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Recipe recipe, List<RecipeIngredient> Ingredients, List<RecipeStep> RecipeSteps, HttpPostedFileBase ImageFile, string Tag)
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
                Recipe oldRecipe = await db.Recipes
                    .Include(r => r.User)
                    .Include(r => r.Reposts)
                    .Include(r => r.Likes)
                    .Include(r => r.Ingredients)
                    .Include(r => r.RecipeSteps)
                    .FirstOrDefaultAsync(r => r.Id == recipe.Id);
                if (oldRecipe == null)
                {
                    return View(recipe);
                }

                if (oldRecipe.UserId != User.Identity.GetUserId() && !isAdmin )
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }


                oldRecipe.UserId = oldRecipe.UserId;
                oldRecipe.Name = recipe.Name;
                oldRecipe.Description = recipe.Description;
                oldRecipe.Tag = recipe.Tag;
                oldRecipe.IsModified = true;

                db.RecipeIngredients.RemoveRange(oldRecipe.Ingredients);
                db.RecipeSteps.RemoveRange(oldRecipe.RecipeSteps);

                foreach (var ingredient in Ingredients)
                {
                    ingredient.RecipeId = oldRecipe.Id;
                    ingredient.Name = ingredient.Name;
                    ingredient.Measurements = ingredient.Measurements;
                    ingredient.Quantity = ingredient.Quantity;

                    db.RecipeIngredients.Add(ingredient);
                }

                oldRecipe.Ingredients = Ingredients;

                foreach (var step in RecipeSteps)
                {
                    step.RecipeId = oldRecipe.Id;
                    step.StepNumber = step.StepNumber;
                    step.Description = step.Description;

                    db.RecipeSteps.Add(step);
                }

                oldRecipe.RecipeSteps = RecipeSteps;

                if (ImageFile != null && ImageFile.ContentLength > 0)
                {

                    var validFileTypes = new List<string>
                    {
                         "image/jpeg", "image/png", "image/gif",
                         "video/mp4", "video/avi", "video/quicktime",
                         "video/webm"
                    };


                    if (!validFileTypes.Contains(ImageFile.ContentType))
                    {
                        ModelState.AddModelError("ImageFile", "Le fichier doit être une image (jpg, png, gif) ou une vidéo (mp4, avi, mov, webm).");
                        ViewBag.UserId = new SelectList(db.Users, "Id", "Name", recipe.UserId);
                        return View(recipe);
                    }

                    if (!string.IsNullOrEmpty(oldRecipe.ContentPath))
                    {
                        string oldFilePath = Server.MapPath(oldRecipe.ContentPath);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    string userFolder = oldRecipe.UserId;
                    string userFolderPath = Server.MapPath($"/Content_Data/Users/{userFolder}");

                    if (!Directory.Exists(userFolderPath))
                    {
                        Directory.CreateDirectory(userFolderPath);
                    }

                    string fileName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                    string fileExtension = Path.GetExtension(ImageFile.FileName);
                    string uniqueFileName = $"{fileName}_{DateTime.Now.Ticks}{fileExtension}";

                    string filePath = Path.Combine(userFolderPath, uniqueFileName);
                    ImageFile.SaveAs(filePath);

                    oldRecipe.ContentPath = $"/Content_Data/Users/{userFolder}/{uniqueFileName}";
                }

                db.Recipes.AddOrUpdate(oldRecipe);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "Feed");
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "Name", recipe.UserId);
            return View(recipe);
        }
        public ActionResult Delete(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Recipe recipe = db.Recipes.Find(id);
            if (recipe == null)
            {
                return HttpNotFound();
            }
            return View(recipe);
        }

        // POST: Recipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Recipe recipe = db.Recipes.Find(id);
            db.Recipes.Remove(recipe);
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

        public async Task<JsonResult> ToggleLike(string recipeId)
        {
            var result = await _recipeRepository.ToggleLike(int.Parse(recipeId), User.Identity.GetUserId());
            var updatedLikesCount = await _recipeRepository.GetLikesCount(int.Parse(recipeId));

            return Json(new { success = result, likesCount = updatedLikesCount });
        }

        [HttpPost]
        public async Task<JsonResult> AddComment(string recipeId, string comment)
        {
            var result = await _recipeRepository.AddComment(int.Parse(recipeId), User.Identity.GetUserId(), comment);

            return Json(new { success = result });
        }
        [HttpPost]
        public async Task<JsonResult> AddRecipeToBook(string recipeId)
        {
            bool result = await _recipeRepository.AddRecipeToBook(int.Parse(recipeId), User.Identity.GetUserId());

            return Json(new { success = result });
        }
        [HttpPost]
        public async Task<JsonResult> RemoveRecipeFromBook(string recipeId)
        {
            bool result = await _recipeRepository.RemoveRecipeFromBook(int.Parse(recipeId), User.Identity.GetUserId());
            return Json(new { success = result });
        }
        [HttpPost]
        public async Task<JsonResult> AnswerComment(string recipeCommentId, string comment)
        {
            var result = await _recipeRepository.AddAnswer(int.Parse(recipeCommentId), User.Identity.GetUserId(), comment);

            return Json(new { success = result });
        }

    public ActionResult _PartialComments(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Recipe recipe = db.Recipes.Find(id);
            if (recipe == null)
            {
                return HttpNotFound();
            }
            List<RecipeComment> comments = new List<RecipeComment>();
            var commentsDB = db.RecipeComments.ToList();
            foreach (var comment in commentsDB)
            {
                if (comment.RecipeId == id)
                {
                    comment.User = db.Users.Find(comment.UserId);
                    comments.Add(comment);
                }
            }
            return PartialView(comments);

        }
        public ActionResult _PartialAnswers(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            RecipeComment recipe = db.RecipeComments.Find(id);
            if (recipe == null)
            {
                return HttpNotFound();
            }

            List<AnswerRecipeComment> Answers = new List<AnswerRecipeComment>();
            var answerDB = db.AnswerRecipeComment.ToList();
            foreach (var answer in answerDB)
            {
                if (answer.recipeCommentId == id)
                {
                    answer.User = db.Users.Find(answer.UserId);
                    Answers.Add(answer);
                }
            }
            return PartialView(Answers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddReview(int recipeId, int stars)
        {
            var userId = User.Identity.GetUserId();
            if (userId == null || recipeId <= 0 || stars < 1 || stars > 5)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var recipe = await db.Recipes
                .Include(r => r.Reviews)
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipe == null)
            {
                return HttpNotFound();
            }

            var existingReview = db.RecipeReviews.FirstOrDefault(r => r.RecipeId == recipeId && r.UserId == userId);
            if (existingReview != null)
            {
                ModelState.AddModelError(string.Empty, "Vous avez déjà évalué cette recette.");
                return RedirectToAction("Details", "Recipe", new { id = recipeId });
            }

            var review = new RecipeReview
            {
                RecipeId = recipeId,
                UserId = userId,
                Stars = stars,
                Date = DateTime.Now
            };

            db.RecipeReviews.Add(review);
            db.SaveChanges();

            return RedirectToAction("Details", "Recipe", new { id = recipeId });
        }


    }
}
