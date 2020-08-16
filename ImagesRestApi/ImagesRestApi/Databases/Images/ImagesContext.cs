using System;
using ImagesRestApi.Databases.Images.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImagesRestApi.Databases.Images
{
    public sealed class ImagesContext : DbContext
    {
        public ImagesContext(DbContextOptions options) : base(options)
        {
            //only for test
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        public DbSet<Image> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Image>(entity =>
            {
                entity.Property(i => i.Id).IsRequired();
                entity.HasKey(i => i.Id);
            });

            #region ImageSeed
            modelBuilder.Entity<Image>().HasData(new Image
            {
                Id = new Guid("00000000-0000-0000-0000-000000000001"), 
                Path = @"Images\00000000-0000-0000-0000-000000000001\original.jpeg"
            });
            #endregion ImageSeed
        }
    }
}
