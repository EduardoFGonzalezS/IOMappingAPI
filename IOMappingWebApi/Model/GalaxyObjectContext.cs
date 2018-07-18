using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace IOMappingWebApi.Model
{
    public class GalaxyObjectContext : DbContext
    {
        public GalaxyObjectContext(DbContextOptions<GalaxyObjectContext> options) : base(options)
        {
        }
        public GalaxyObjectContext()
        { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { }

        public DbSet<InstanceContent> InstanceContent { get; set; }
        public DbSet<Attribute> Attribute { get; set; }
        public DbSet<IOTag> IOTag { get; set; }
        public DbSet<PLCTag> PLCTag { get; set; }
        public DbSet<Instance> Instance { get; set; }
        public DbSet<PLC> PLC { get; set; }
    }


}