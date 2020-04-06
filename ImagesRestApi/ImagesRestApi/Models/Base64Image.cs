using System;

namespace ImagesRestApi.Models
{
    public class Base64Image: Base64File
    {
        public Guid Id { get; set; }
    }
    public class Base64File
    {
        public string Base64 { get; set; }
    }
}