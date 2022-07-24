namespace MyAbilityFirst.Infrastructure.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCareWorkerIDInJobTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Job", "CareWorkerId", c => c.Int());
            CreateIndex("dbo.Job", "CareWorkerId");
            AddForeignKey("dbo.Job", "CareWorkerId", "dbo.CareWorker", "ID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Job", "CareWorkerId", "dbo.CareWorker");
            DropIndex("dbo.Job", new[] { "CareWorkerId" });
            DropColumn("dbo.Job", "CareWorkerId");
        }
    }
}
