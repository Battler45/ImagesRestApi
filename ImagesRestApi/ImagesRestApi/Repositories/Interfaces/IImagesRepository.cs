using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImagesRestApi.DTO;

namespace ImagesRestApi.Repositories.Interfaces
{
    public interface IImagesRepository
    {
        Task<int> SaveImage(ImageDTO imageDto);
        Task<ImageDTO> GetImageAsync(Guid id);
        Task<List<ImageDTO>> GetImagesAsync(IEnumerable<Guid> ids);
        Task<int> SaveImages(IEnumerable<ImageDTO> imagesDto);
        Task<int> DeleteImage(Guid imageId); 
        Task<int> DeleteImages(IEnumerable<Guid> imagesIds);
        Task<int> UpdateImages(IEnumerable<ImageDTO> imagesDto);
        Task<int> UpdateImage(ImageDTO imageDto);
    }
}