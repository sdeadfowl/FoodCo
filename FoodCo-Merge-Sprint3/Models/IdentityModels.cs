using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace FoodCo.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string Lastname { get; set; }
        public string ProfilePicturePath { get; set; }
        public string Biography { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public bool HasNotification { get; set; }
        public bool Blocked { get; set; }
        public bool IsAccountBlocked { get; set; } = false;
        public bool IsPrivate { get; set; } = false;

        public virtual ICollection<Chat> Chats { get; set; }
        public virtual ICollection<Follow> Followers { get; set; }
        public virtual ICollection<Follow> Followings { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Recipe> Recipes { get; set; }
        public virtual ICollection<RecipeReview> RecipeReviews { get; set; }
        public virtual ICollection<AnswerRecipeComment> AnswerRecipeComment { get; set; }
        public virtual ICollection<AnswerComments> AnswerComment { get; set; }
        public virtual ICollection<RecipeBook> RecipeBooks { get; set; }
        public virtual ICollection<Block> Blocks { get; set; }
        public virtual ICollection<Admin> Admins { get; set; }




        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            if (!string.IsNullOrEmpty(ProfilePicturePath))
            {
                userIdentity.AddClaim(new Claim("ProfilePicturePath", ProfilePicturePath));
            }

            return userIdentity;
        }

    }


    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        //Post
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<PostRepost> PostReposts { get; set; }
        public DbSet<AnswerComments> AnswerComments { get; set; }


        //Recipe
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<RecipeStep> RecipeSteps { get; set; }
        public DbSet<RecipeComment> RecipeComments { get; set; }
        public DbSet<RecipeLike> RecipeLikes { get; set; }
        public DbSet<RecipeRepost> RecipeReposts { get; set; }
        public DbSet<RecipeReview> RecipeReviews { get; set; }
        public DbSet<AnswerRecipeComment> AnswerRecipeComment { get; set; }
        public DbSet<RecipeBook> RecipeBooks { get; set; }

        public DbSet<Block> Blocks { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public Follow follow { get; set; }

        public FollowRepository FollowRepositories { get; set; }
        //Chat
        public DbSet<Chat> Chats { get; set; }

        //Admin
        public DbSet<Admin> Admin { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogins");

            //Relationships
            modelBuilder.Entity<Post>()
                .HasRequired(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .WillCascadeOnDelete(true);

            // Recipe relationship
            modelBuilder.Entity<Recipe>()
                .HasRequired(r => r.User)
                .WithMany(u => u.Recipes)
                .HasForeignKey(r => r.UserId)
                .WillCascadeOnDelete(true);
            // RecipeIngredient relationship
            modelBuilder.Entity<RecipeIngredient>()
                .HasRequired(ri => ri.Recipe)
                .WithMany(r => r.Ingredients)
                .HasForeignKey(ri => ri.RecipeId)
                .WillCascadeOnDelete(true);
            // RecipeStep relationship
            modelBuilder.Entity<RecipeStep>()
                .HasRequired(rs => rs.Recipe)
                .WithMany(r => r.RecipeSteps)
                .HasForeignKey(rs => rs.RecipeId)
                .WillCascadeOnDelete(true);
            // RecipeReview Relationship
            modelBuilder.Entity<RecipeReview>()
                .HasRequired(rr => rr.Recipe)
                .WithMany(r => r.Reviews)
                .HasForeignKey(rr => rr.RecipeId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<RecipeReview>()
                .HasRequired(rr => rr.User)
                .WithMany(u => u.RecipeReviews)
                .HasForeignKey(rr => rr.UserId)
                .WillCascadeOnDelete(false);

            // Recipe Answers Relationchip
            modelBuilder.Entity<AnswerRecipeComment>()
                .HasRequired(f => f.RecipeComment)
                .WithMany(u => u.Comments)
                .HasForeignKey(f => f.recipeCommentId)
                .WillCascadeOnDelete(false);

            // Post Answers Relationchip
            modelBuilder.Entity<AnswerComments>()
                .HasRequired(f => f.PostComment)
                .WithMany(u => u.Comments)
                .HasForeignKey(f => f.postCommentId)
                .WillCascadeOnDelete(false);

            // Follow relationship

            modelBuilder.Entity<Follow>()
                .HasKey(f => new { f.FollowerId, f.FollowedUserId });

            modelBuilder.Entity<Follow>()
                .HasRequired(f => f.Follower)
                .WithMany(u => u.Followings)
                .HasForeignKey(f => f.FollowerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Follow>()
                .HasRequired(f => f.FollowedUser)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowedUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Block>()
                .HasKey(b => new { b.BlockerId, b.BlockedId });
            modelBuilder.Entity<Block>()
                .HasRequired(b => b.Blocker)
                .WithMany(u => u.Blocks)
                .HasForeignKey(b => b.BlockedId)
                .WillCascadeOnDelete(false);

            //Admin Relatioship
            //modelBuilder.Entity<Admin>()
            //    .HasKey(b => new { b.UserId});

            modelBuilder.Entity<Admin>()
                .HasRequired(a => a.User)
                .WithMany(a => a.Admins)
                .HasForeignKey(f => f.UserId)
                .WillCascadeOnDelete(true);


        }
        
    }
}