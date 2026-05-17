namespace FoodCo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class blocks : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Blocks",
                c => new
                    {
                        BlockerId = c.String(nullable: false, maxLength: 128),
                        BlockedId = c.String(nullable: false, maxLength: 128),
                        BlockDate = c.DateTime(nullable: false),
                        BlockStatus = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.BlockerId, t.BlockedId })
                .ForeignKey("dbo.Users", t => t.BlockedId)
                .Index(t => t.BlockedId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Blocks", "BlockedId", "dbo.Users");
            DropIndex("dbo.Blocks", new[] { "BlockedId" });
            DropTable("dbo.Blocks");
        }
    }
}
