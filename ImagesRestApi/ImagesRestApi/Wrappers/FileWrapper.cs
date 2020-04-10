using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImagesRestApi.Wrappers
{
    public interface IFileWrapper
    {
        FileStream Create(string path);

        Task<byte[]> ReadAllBytesAsync(string path,
            CancellationToken cancellationToken = default);

        bool Exists(string path);
    }

    public class FileWrapper : IFileWrapper
    {
        public FileStream Create(string path) => File.Create(path);
        public bool Exists(string path) => File.Exists(path);
        public Task<byte[]> ReadAllBytesAsync(string path,
            CancellationToken cancellationToken = default) =>
            File.ReadAllBytesAsync(path, cancellationToken);
    }
}
