using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImagesRestApi.Wrappers
{
    public interface IFileWrapper
    {
        FileStream Create(string path);

        Task<byte[]> ReadAllBytesAsync(string path,
            CancellationToken cancellationToken = default);
    }

    public class FileWrapper : IFileWrapper
    {
        public FileStream Create(string path) => File.Create(path);

        public Task<byte[]> ReadAllBytesAsync(string path,
            CancellationToken cancellationToken = default) =>
            File.ReadAllBytesAsync(path, cancellationToken);
    }
}
