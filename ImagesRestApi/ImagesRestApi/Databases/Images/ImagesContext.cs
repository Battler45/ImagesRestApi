using ImagesRestApi.Databases.Images.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImagesRestApi.Databases.Images
{
    public sealed class ImagesContext : DbContext
    {
        public ImagesContext(DbContextOptions options) : base(options)
        {
            //only for test
            Database.EnsureCreated();
        }
        public DbSet<Image> Images { get; set; }
    }
}
