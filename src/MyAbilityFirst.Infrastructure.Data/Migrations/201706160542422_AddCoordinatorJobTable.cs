namespace MyAbilityFirst.Infrastructure.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCoordinatorJobTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CoordinatorJob",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CoordinatorID = c.Int(nullable: false),
                        JobID = c.Int(nullable: false),
                        Active = c.Boolean(nullable: false),
                        Comment = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Coordinator", t => t.CoordinatorID)
                .Index(t => t.CoordinatorID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CoordinatorJob", "CoordinatorID", "dbo.Coordinator");
            DropIndex("dbo.CoordinatorJob", new[] { "CoordinatorID" });
            DropTable("dbo.CoordinatorJob");
        }
    }
}
