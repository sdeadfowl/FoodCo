namespace FoodCo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class blocked : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Blocks", "BlockedId", "dbo.Users");
            DropIndex("dbo.Blocks", new[] { "BlockedUser_Id" });
            DropColumn("dbo.Blocks", "BlockerId");
            RenameColumn(table: "dbo.Blocks", name: "BlockedUser_Id", newName: "BlockedId");
            RenameColumn(table: "dbo.Blocks", name: "BlockedId", newName: "BlockerId");
            RenameIndex(table: "dbo.Blocks", name: "IX_BlockedId", newName: "IX_BlockerId");
            DropPrimaryKey("dbo.Blocks");
            AlterColumn("dbo.Blocks", "BlockedId", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.Blocks", new[] { "BlockerId", "BlockedId" });
            CreateIndex("dbo.Blocks", "BlockedId");
            AddForeignKey("dbo.Blocks", "BlockerId", "dbo.Users", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Blocks", "BlockerId", "dbo.Users");
            DropIndex("dbo.Blocks", new[] { "BlockedId" });
            DropPrimaryKey("dbo.Blocks");
            AlterColumn("dbo.Blocks", "BlockedId", c => c.String(maxLength: 128));
            AddPrimaryKey("dbo.Blocks", new[] { "BlockerId", "BlockedId" });
            RenameIndex(table: "dbo.Blocks", name: "IX_BlockerId", newName: "IX_BlockedId");
            RenameColumn(table: "dbo.Blocks", name: "BlockerId", newName: "BlockedId");
            RenameColumn(table: "dbo.Blocks", name: "BlockedId", newName: "BlockedUser_Id");
            AddColumn("dbo.Blocks", "BlockerId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Blocks", "BlockedUser_Id");
            AddForeignKey("dbo.Blocks", "BlockedId", "dbo.Users", "Id");
        }
    }
}
