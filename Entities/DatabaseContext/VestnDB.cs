using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using Entity;


namespace Entity
{
    public class VestnDB : DbContext
    {
        public VestnDB() : base("VestnDB")
        {

        }
        public DbSet<User> users { get; set; }
        public DbSet<fTag> fTag { get; set; }
        public DbSet<sTag> sTag { get; set; }
        public DbSet<Project> projects { get; set; }
        public DbSet<ProjectElement> projectElements { get; set; }
        public DbSet<Log> logs { get; set; }
        public DbSet<Feedback> feedback { get; set; }
        public DbSet<Analytics> analytics { get; set; }
        public DbSet<UserAgreement> userAgreements { get; set; }
        public DbSet<ProjectTags> projectTags { get; set; }
        public DbSet<UserTags> userTags { get; set; }
    }
}
