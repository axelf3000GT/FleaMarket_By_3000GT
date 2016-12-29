namespace FleaMarket.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class aspnetFleaMarket20161202021115 : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.AdSorts");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.AdSorts",
                c => new
                    {
                        AdSortId = c.Int(nullable: false, identity: true),
                        AdSortName = c.String(),
                    })
                .PrimaryKey(t => t.AdSortId);
            
        }
    }
}
