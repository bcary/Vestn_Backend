﻿using System;
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
           // this.Configuration.LazyLoadingEnabled = false;
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
        public DbSet<Authentication> authentication { get; set; }
        public DbSet<Experience> experience{ get; set; }
        public DbSet<Reference> reference { get; set; }
        public DbSet<Prop> prop { get; set; }
        public DbSet<Activity> activity { get; set; }
        public DbSet<Network> networks { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Network>()
                .HasMany(n => n.networkUsers)
                .WithMany(u => u.networks)
                .Map(m =>
                    {
                        m.MapLeftKey("NetworkId");
                        m.MapRightKey("UserId");
                        m.ToTable("UserNetworks");
                    });

            modelBuilder.Entity<Network>()
                .HasMany(n => n.admins)
                .WithMany(u => u.adminNetworks)
                .Map(m =>
                    {
                        m.MapLeftKey("AdminNetworkId");
                        m.MapRightKey("AdminId");
                        m.ToTable("AdminNetworks");
                    });

            //modelBuilder.Entity<Network_TopNetwork>()
            //    .HasMany(s => s.subNetworks)
            //    .WithOptional()
            //    .Map(m => m.MapKey("TopNetworkId"));

            //modelBuilder.Entity<Network_SubNetwork>()
            //    .HasMany(g => g.groups)
            //    .WithOptional()
            //    .Map(m => m.MapKey("SubNetworkId"));
                

            //base.OnModelCreating(modelBuilder);
        }
    }
}
