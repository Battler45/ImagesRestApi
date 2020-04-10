using System.Collections.Generic;
using System.Threading.Tasks;
using ImagesRestApi.Models;

namespace ImagesRestApi.Services.Interfaces
{
    public interface IUploader
    {
        Task<byte[]> UploadContent(string url);
        Task<File> UploadFile(string url);
        Task<byte[][]> UploadContents(IEnumerable<string> urls);
        Task<List<File>> UploadFiles(IEnumerable<string> urls);
    }
}