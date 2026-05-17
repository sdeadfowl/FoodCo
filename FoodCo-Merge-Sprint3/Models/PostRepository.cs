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
    public class PostRepository
    {

        private ApplicationDbContext DB;

        public PostRepository(ApplicationDbContext context)
        {
            DB = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Post> GetPostById(int id)
        {
            var post = await DB.Posts.FirstOrDefaultAsync(p=>p.Id == id);

            return post;
        }

        public async Task<List<Post>> GetPostsByUsername(string username)
        {
            var user = await DB.Users.FirstOrDefaultAsync(u => u.UserName == username);

            if (user != null)
            {
                var posts = await DB.Posts.Where(p => p.UserId == user.Id).ToListAsync();
                return posts;
            }


            return new List<Post>();
        }

        public async Task<List<Post>> GetPostsByUserId(string id)
        {
            var user = await DB.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user != null)
            {
                var posts = await DB.Posts.Where(p => p.UserId == user.Id).ToListAsync();
                return posts;
            }


            return new List<Post>();
        }

        public async Task<bool> ToggleLike(int postId, string userId)
        {
            var post = await DB.Posts
                .Include(p => p.Likes) 
                .FirstOrDefaultAsync(p => p.Id == postId);

            var user = await DB.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (post == null || user == null)
            {
                return false;
            }

            var existingLike = post.Likes.FirstOrDefault(l => l.UserId == userId);

            if (existingLike == null)
            {
                // Add like
                var newLike = new PostLike
                {
                    UserId = userId,
                    PostId = postId
                };
                DB.PostLikes.Add(newLike);
                
                await DB.SaveChangesAsync(); 
                return true;
            }
            else
            {
                // Remove like
                DB.PostLikes.Remove(existingLike);
                await DB.SaveChangesAsync(); 
                return false;
            }
        }


        public async Task<int> GetLikesCount(int postId)
        {
            var post = await DB.Posts.FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
            {
                return 0;
            }

            return await DB.PostLikes.CountAsync(l=> l.PostId == postId);
        }

        public async Task<bool> AddComment(int postId, string userId, string commentText)
        {
            try
            {
                var post = await DB.Posts
                    .Include(r => r.Comments)
                    .FirstOrDefaultAsync(r => r.Id == postId);

                var user = await DB.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (post == null || user == null)
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

                var comment = new PostComment
                {
                    PostId = postId,
                    UserId = userId,
                    Text = commentText,
                    Date = DateTime.Now,
                    isMention = isMention 
                };

                post.Comments.Add(comment);

                await DB.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> AddAnswer(int postCommentId, string userId, string commentText)
        {
            try
            {
                var post = await DB.PostComments
                    .Include(r => r.Comments)
                    .FirstOrDefaultAsync(r => r.Id == postCommentId);

                var user = await DB.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (post == null || user == null)
                {
                    return false;
                }

                var comment = new AnswerComments
                {
                    postCommentId = postCommentId,
                    UserId = userId,
                    Text = commentText,
                    Date = DateTime.Now
                };

                post.Comments.Add(comment);
                await DB.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}