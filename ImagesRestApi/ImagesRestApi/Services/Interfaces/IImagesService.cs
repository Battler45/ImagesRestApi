using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using ImagesRestApi.DTO;
using ImagesRestApi.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace ImagesRestApi.Services.Interfaces
{
    public interface IImagesService
    {
        Task<ImageDTO> SaveImage(PipeReader reader, string contentType);
        Task<Image> GetImageAsync(Guid id);
        Task<List<ImageDTO>> SaveImages(MultipartReader requestReader);
        Task<int> DeleteImage(Guid imageId);
        Task<int> DeleteImages(IEnumerable<Guid> imagesIds);
        Task<ImageDTO> SaveImage(File file);
        Task<List<ImageDTO>> SaveImages(IEnumerable<File> files);
        Task<List<ImageDTO>> UpdateImages(IEnumerable<Image> files);
        Task<ImageDTO> UpdateImage(Image file);
        Task<ImageDTO> UpdateImage(PipeReader reader, Guid imageId, string contentType);
        Task<ImageDTO> UpdateImage(MultipartReader requestReader, Guid imageId);
    }
}