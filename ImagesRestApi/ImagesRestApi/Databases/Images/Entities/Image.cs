using System;

namespace ImagesRestApi.Databases.Images.Entities
{
    public class Image
    {
        //[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public Guid Id { get; set; }
        public string Path { get; set; }
        //public string Name { get; set; }
    }
}