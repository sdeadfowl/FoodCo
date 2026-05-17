namespace FoodCo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class recipebook : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RecipeBooks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        RecipeId = c.String(),
                        recipe_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Recipes", t => t.recipe_Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.recipe_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RecipeBooks", "UserId", "dbo.Users");
            DropForeignKey("dbo.RecipeBooks", "recipe_Id", "dbo.Recipes");
            DropIndex("dbo.RecipeBooks", new[] { "recipe_Id" });
            DropIndex("dbo.RecipeBooks", new[] { "UserId" });
            DropTable("dbo.RecipeBooks");
        }
    }
}
