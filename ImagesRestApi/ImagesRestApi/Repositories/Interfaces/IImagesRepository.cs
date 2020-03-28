using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImagesRestApi.DTO;

namespace ImagesRestApi.Repositories.Interfaces
{
    public interface IImagesRepository
    {
        Task<ImageDTO> GetImageAsync(Guid id);
        Task<int> SaveImages(IEnumerable<ImageDTO> imagesDto);
        Task<int> DeleteImage(Guid imageId); 
        Task<int> DeleteImages(IEnumerable<Guid> imagesIds);
    }
}