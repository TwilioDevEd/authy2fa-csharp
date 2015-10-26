namespace Authy2FA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAuthyStatusToUsers : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "AuthyStatus", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "AuthyStatus");
        }
    }
}
