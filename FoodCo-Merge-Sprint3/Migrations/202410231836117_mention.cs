namespace FoodCo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mention : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PostComments", "isMention", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PostComments", "isMention");
        }
    }
}
