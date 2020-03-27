using ImagesRestApi.Databases.Images.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace ImagesRestApi.Databases.Images
{
    public sealed class ImagesContext : DbContext
    {
        private readonly IConfiguration _config;

        public ImagesContext(DbContextOptions options, IConfiguration config) : base(options)
        {
            _config = config;
            Database.EnsureCreated();
        }

        public DbSet<Image> Images { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("Images");
        }

        protected override void OnModelCreating(ModelBuilder bldr)
        {
            bldr.Entity<Image>()
              .HasData(new
              {
                  Id = new Guid("00000000-0000-0000-0000-000000000001"),//32:0 00000000-0000-0000-0000-000000000000 //Guid.NewGuid(),
                  Path = @"C:\Games\PetProjects\TestTasks\ImagesRestApi\ImagesRestApi\Images\lights.jpg",
                  Name = "lights"
              });
        }
    }
}
