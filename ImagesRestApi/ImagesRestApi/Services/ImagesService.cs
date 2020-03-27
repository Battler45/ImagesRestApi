using AutoMapper;
using ImagesRestApi.Models;
using ImagesRestApi.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using ImagesRestApi.Services.Interfaces;

namespace ImagesRestApi.Services
{
    public class ImagesService : IImagesService
    {
        private readonly IImagesRepository _images;
        private readonly ILogger<ImagesService> _logger;

        public ImagesService(IImagesRepository images, ILogger<ImagesService> logger)
        {
            _images = images ?? throw new ArgumentNullException(nameof(images)); 
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Image> GetImageAsync(Guid id)
        {
            var imageDto = await _images.GetImageAsync(id);
            if (imageDto == null || Directory.Exists(imageDto.Path)) return null;
            var imageFile = await File.ReadAllBytesAsync(imageDto.Path);
            return new Image()
            {
                //Name = imageDto.Name,
                Id =  imageDto.Id,
                File =  imageFile
            };
        }
    }
}
