namespace FoodCo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Friends : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Blocked", c => c.Boolean(nullable: false));
            AddColumn("dbo.Follows", "Id", c => c.String());
            AddColumn("dbo.Follows", "CreationDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Follows", "FriendshipStatus", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Follows", "FriendshipStatus");
            DropColumn("dbo.Follows", "CreationDate");
            DropColumn("dbo.Follows", "Id");
            DropColumn("dbo.Users", "Blocked");
        }
    }
}
