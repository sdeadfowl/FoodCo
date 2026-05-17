namespace FoodCo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cascade : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Admins", "UserId", "dbo.Users");
            AddForeignKey("dbo.Admins", "UserId", "dbo.Users", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Admins", "UserId", "dbo.Users");
            AddForeignKey("dbo.Admins", "UserId", "dbo.Users", "Id");
        }
    }
}
