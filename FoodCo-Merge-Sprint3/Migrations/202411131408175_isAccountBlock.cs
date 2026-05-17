namespace FoodCo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class isAccountBlock : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "IsAccountBlocked", c => c.Boolean(nullable: false));
            DropColumn("dbo.Users", "IsBlock");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "IsBlock", c => c.Boolean(nullable: false));
            DropColumn("dbo.Users", "IsAccountBlocked");
        }
    }
}
