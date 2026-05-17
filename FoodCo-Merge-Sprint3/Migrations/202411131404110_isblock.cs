namespace FoodCo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class isblock : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "IsBlock", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "IsBlock");
        }
    }
}
