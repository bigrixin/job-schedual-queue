namespace MyAbilityFirst.Infrastructure.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddJobStatusInJobTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Job", "Status", c => c.Int(nullable: false));
            DropColumn("dbo.Job", "JobStatus");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Job", "JobStatus", c => c.String());
            DropColumn("dbo.Job", "Status");
        }
    }
}
