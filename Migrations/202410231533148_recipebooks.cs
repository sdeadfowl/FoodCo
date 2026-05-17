namespace FoodCo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class recipebooks : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.RecipeBooks", "recipe_Id", "dbo.Recipes");
            DropIndex("dbo.RecipeBooks", new[] { "recipe_Id" });
            DropColumn("dbo.RecipeBooks", "RecipeId");
            DropColumn("dbo.RecipeBooks", "recipe_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RecipeBooks", "recipe_Id", c => c.Int());
            AddColumn("dbo.RecipeBooks", "RecipeId", c => c.String());
            CreateIndex("dbo.RecipeBooks", "recipe_Id");
            AddForeignKey("dbo.RecipeBooks", "recipe_Id", "dbo.Recipes", "Id");
        }
    }
}
