using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FoodCo.Migrations;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace FoodCo.Models
{
    public class RecipeRepository
    {

        private ApplicationDbContext DB;

        public RecipeRepository(ApplicationDbContext context)
        {
            DB = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Recipe> GetRecipeById(int id)
        {
            var recipe = await DB.Recipes.FirstOrDefaultAsync(p=>p.Id == id);

            return recipe;
        }

        public async Task<List<Recipe>> GetRecipesByUsername(string username)
        {
            var user = await DB.Users.FirstOrDefaultAsync(u => u.UserName == username);

            if (user != null)
            {
                var recipes = await DB.Recipes.Where(p => p.UserId == user.Id).ToListAsync();
                return recipes;
            }


            return new List<Recipe>();
        }

        public async Task<List<Recipe>> GetRecipesByUserId(string id)
        {
            var user = await DB.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user != null)
            {
                var recipes = await DB.Recipes.Where(p => p.UserId == user.Id).ToListAsync();
                return recipes;
            }


            return new List<Recipe>();
        }

        public async Task<bool> ToggleLike(int recipeId, string userId)
        {
            var recipe = await DB.Recipes
                .Include(r => r.Likes) 
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            var user = await DB.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (recipe == null || user == null)
            {
                return false;
            }

            var existingLike = recipe.Likes.FirstOrDefault(l => l.UserId == userId);

            if (existingLike == null)
            {
                // Add like
                var newLike = new RecipeLike
                {
                    UserId = userId,
                    RecipeId = recipeId
                };
                DB.RecipeLikes.Add(newLike);
                await DB.SaveChangesAsync(); 
                return true;
            }
            else
            {
                // Remove like
                DB.RecipeLikes.Remove(existingLike);
                await DB.SaveChangesAsync(); 
                return false;
            }
        }


        public async Task<int> GetLikesCount(int recipeId)
        {
            var recipe = await DB.Recipes.FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipe == null)
            {
                return 0;
            }

            return await DB.RecipeLikes.CountAsync(l=> l.RecipeId == recipeId);
        }


        public async Task<bool> AddComment(int recipeId, string userId, string commentText)
        {
            try
            {
                var recipe = await DB.Recipes
                    .Include(r => r.Comments)
                    .FirstOrDefaultAsync(r => r.Id == recipeId);

                var user = await DB.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (recipe == null || user == null)
                {
                    return false;
                }

                var isMention = false;
                var words = commentText.Split(' ');

                foreach (var word in words)
                {
                    if (word.StartsWith("@"))
                    {
                        var mentionedUsername = word.TrimStart('@');

                        var mentionedUser = await DB.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == mentionedUsername.ToLower());

                        if (mentionedUser != null)
                        {
                            isMention = true;
                            break;
                        }
                    }
                }

                var comment = new RecipeComment
                {
                    RecipeId = recipeId,
                    UserId = userId,
                    Text = commentText,
                    Date = DateTime.Now,
                    isMention = isMention
                };

                recipe.Comments.Add(comment);
                await DB.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> AddAnswer(int recipeCommentId, string userId, string commentText)
        {
            try
            {
                var recipe = await DB.RecipeComments
                    .Include(r => r.Comments)
                    .FirstOrDefaultAsync(r => r.Id == recipeCommentId);

                var user = await DB.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (recipe == null || user == null)
                {
                    return false;
                }

                var comment = new AnswerRecipeComment
                {
                    recipeCommentId = recipeCommentId,
                    UserId = userId,
                    Text = commentText,
                    Date = DateTime.Now
                };

                recipe.Comments.Add(comment);
                await DB.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> AddRecipeToBook(int recipeId, string userId)
        {
            try
            {
                var recipe = await DB.Recipes
                    .Include(r => r.User)
                    .Include(r => r.RecipeSteps)
                    .Include(r => r.Ingredients)
                    .FirstOrDefaultAsync(r => r.Id == recipeId);

                var user = await DB.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var existingRecipeBook = await DB.RecipeBooks.FirstOrDefaultAsync(r => r.Id == recipeId);
                if (recipe == null || user == null)
                {
                    return false;
                }
                
               if(recipe.User.RecipeBooks == null)
               {
                    var recibeBook = new RecipeBook
                    {
                        recipes = recipe,
                        RecipeId = recipeId,
                        UserId = userId,
                        User = user,
                    };
                   
                    user.RecipeBooks.Add(recibeBook);
                    await DB.SaveChangesAsync();

                    return true;
               }
               if(!user.RecipeBooks.ToList().Any(r => r.RecipeId == recipe.Id && r.UserId == userId))
               {
                    var recibeBook = new RecipeBook
                    {
                        recipes = recipe,
                        RecipeId = recipeId,
                        UserId = userId,
                        User = user,
                    };

                    user.RecipeBooks.Add(recibeBook);
                    await DB.SaveChangesAsync();

                    return true;
               }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> RemoveRecipeFromBook(int recipeId, string userId)
        {
            try
            {
                var recipe = await DB.Recipes
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Id == recipeId);

                var user = await DB.Users
                    .Include(u => u.RecipeBooks)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (recipe == null || user == null)
                {
                    return false;
                }


                var recipeBookEntry = user.RecipeBooks.FirstOrDefault(r => r.RecipeId == recipeId);

                if (recipeBookEntry != null)
                {
                    user.RecipeBooks.Remove(recipeBookEntry);
                    await DB.SaveChangesAsync();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

    }
}