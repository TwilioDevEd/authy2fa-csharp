namespace WebApplication19.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameToken : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "AuthyId", c => c.String());
            DropColumn("dbo.AspNetUsers", "Token");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "Token", c => c.String());
            DropColumn("dbo.AspNetUsers", "AuthyId");
        }
    }
}
