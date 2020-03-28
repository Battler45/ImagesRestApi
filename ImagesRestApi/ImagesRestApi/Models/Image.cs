using System;

namespace ImagesRestApi.Models
{
    public class Image
    {
        //public string Name { get; set; }
        public Guid Id { get; set; }
        //[Required]
        //[MinLength(12)]
        public byte[] File { get; set; }

        public Image DeleteMetadata()
        {
            throw  new NotImplementedException();
        }
    }
}
