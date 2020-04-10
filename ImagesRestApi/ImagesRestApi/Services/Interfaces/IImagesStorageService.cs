using System;
using System.Threading.Tasks;
using ImagesRestApi.DTO;
using ImagesRestApi.Models;

namespace ImagesRestApi.Services.Interfaces
{
    public interface IImagesStorageService
    {
        Task<byte[]> GetImage(ImageDTO image);
        Task<ImageDTO> SaveImage(ProcessedStreamedFile file, Guid fileId);
        void DeleteImageDirectory(ImageDTO image);
    }
}