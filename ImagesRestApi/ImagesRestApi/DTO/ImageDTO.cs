using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImagesRestApi.DTO
{
    public class ImageDTO
    {
        public  Guid Id { get; set; }
        public string Path { get; set; }
        //public string Name { get; set; }
    }
}
