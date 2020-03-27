using System;
using System.Threading.Tasks;
using ImagesRestApi.DTO;

namespace ImagesRestApi.Repositories.Interfaces
{
    public interface IImagesRepository
    {
        Task<ImageDTO> GetImageAsync(Guid id);
    }
}