namespace ImagesRestApi.Models
{
    public class File
    {
        public byte[] Content { get;  set; }
        public string Extension { get; set; }
        public string ContentType { get; set; }
    }
}