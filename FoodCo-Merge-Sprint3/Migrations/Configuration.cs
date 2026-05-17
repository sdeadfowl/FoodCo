namespace FoodCo.Migrations
{
    using FoodCo.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<FoodCo.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        //protected override void Seed(FoodCo.Models.ApplicationDbContext context)
        //{
        //    var user = new ApplicationUser
        //    {
        //        UserName = "admin",
        //        Name = "Admin",
        //        Lastname = "User",
        //        Email = "admin@example.com"
        //    };
        //    context.Users.AddOrUpdate(u => u.UserName, user);
        //    context.SaveChanges();

        //    var post = new Models.Post
        //    {
        //        Text = "Test #1 [Seed]",
        //        Content = null,
        //        UserId = user.Id
        //    };
        //    context.Posts.AddOrUpdate(p => p.Text, post);
        //    context.SaveChanges();
        //}
    }
}
