namespace FoodCo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addmigrationprivate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "IsPrivate", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "IsPrivate");
        }
    }
}
