using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace FleaMarket.Models
{
    public class FleaContext : DbContext
    {
        public FleaContext()  : base("DefaultConnection")   { }

        public DbSet<Ad> Ads { get; set; }
        public DbSet<AdCat> AdCats { get; set; }
        public DbSet<AdType> AdTypes { get; set; }
        public DbSet<AdUser> AdUsers { get; set; }
        public DbSet<AdLocation> AdLocations { get; set; }
        public DbSet<News> Newss { get; set; }
        public DbSet<NewsCat> NewsCats { get; set; }
        public DbSet<AdUserdComplaint> AdUserdComplaints { get; set; }
        public DbSet<Op> Ops { get; set; }
    }
     
}