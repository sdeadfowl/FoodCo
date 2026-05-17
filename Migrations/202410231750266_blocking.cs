namespace FoodCo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class blocking : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Blocks", "BlockedUser_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Blocks", "BlockedUser_Id");
            AddForeignKey("dbo.Blocks", "BlockedUser_Id", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Blocks", "BlockedUser_Id", "dbo.Users");
            DropIndex("dbo.Blocks", new[] { "BlockedUser_Id" });
            DropColumn("dbo.Blocks", "BlockedUser_Id");
        }
    }
}
