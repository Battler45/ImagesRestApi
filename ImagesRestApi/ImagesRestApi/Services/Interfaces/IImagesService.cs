using System;
using System.Threading.Tasks;
using ImagesRestApi.Models;

namespace ImagesRestApi.Services.Interfaces
{
    public interface IImagesService
    {
        Task<Image> GetImageAsync(Guid id);
    }
}