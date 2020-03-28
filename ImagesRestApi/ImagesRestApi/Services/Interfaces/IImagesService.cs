using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImagesRestApi.DTO;
using ImagesRestApi.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace ImagesRestApi.Services.Interfaces
{
    public interface IImagesService
    {
        Task<Image> GetImageAsync(Guid id);
        Task<List<ImageDTO>> SaveImages(MultipartReader requestReader);
        Task<int> DeleteImage(Guid imageId);
        Task<int> DeleteImages(IEnumerable<Guid> imagesIds);
    }
}