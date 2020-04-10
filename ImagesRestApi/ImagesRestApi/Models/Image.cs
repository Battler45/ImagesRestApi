using System;

namespace ImagesRestApi.Models
{
    public class Image: File
    {
        //public string Name { get; set; }
        public Guid Id { get; set; }
        public string Uri { get; set; }
        public Image DeleteMetadata()
        {
            throw  new NotImplementedException();
        }
    }
}
