using System.IO;

namespace ImagesRestApi.Wrappers
{
    public interface IDirectoryWrapper
    {
        DirectoryInfo CreateDirectory(string path);
        void Delete(string path, bool recursive = false);
        bool Exists(string path);
    }

    public class DirectoryWrapper : IDirectoryWrapper
    {
        public DirectoryInfo CreateDirectory(string path) => Directory.CreateDirectory(path);
        public void Delete(string path, bool recursive = false) => Directory.Delete(path, recursive);
        public bool Exists(string path) => Directory.Exists(path);
    }
}
