using ImagesRestApi.Models;
using ImagesRestApi.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImagesRestApi.DTO;
using ImagesRestApi.Services.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using ImagesRestApi.Utilities;
using ImagesRestApi.Wrappers;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;

namespace ImagesRestApi.Services
{
    public class ImagesService : IImagesService
    {
        private readonly IImagesRepository _images;
        private readonly ILogger<ImagesService> _logger;
        private readonly IDirectoryWrapper _directory;
        private readonly IFileWrapper _file;

        private readonly List<string> _permittedExtensions;
        private readonly long _fileSizeLimit;
        private readonly string _targetFilePath;

        public ImagesService(IImagesRepository images, ILogger<ImagesService> logger, IConfiguration config, IDirectoryWrapper directory, IFileWrapper _file)
        {
            _images = images ?? throw new ArgumentNullException(nameof(images)); 
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            _file = _file ?? throw new ArgumentNullException(nameof(_file));

            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");

            // To save physical files to a path provided by configuration:
            _targetFilePath = config.GetValue<string>("StoredFilesPath");

            _permittedExtensions = config.GetSection("PermittedExtensions").AsEnumerable()
                .Where(p => p.Value != null)
                .Select(p => p.Value)
                .ToList();
        }

        public async Task<Image> GetImageAsync(Guid id)
        {
            var imageDto = await _images.GetImageAsync(id);
            if (imageDto == null || _directory.Exists(imageDto.Path)) return null;
            var imageFile = await _file.ReadAllBytesAsync(imageDto.Path);
            return new Image()
            {
                //Name = imageDto.Name,
                Id =  imageDto.Id,
                File =  imageFile
            };
        }

        public async Task<List<ImageDTO>> SaveImages(MultipartReader requestReader)
        {
            var images = new List<ImageDTO>();
            var section = await requestReader.ReadNextSectionAsync();
            while (section != null)
            {
                var image = await SaveSection(section, _permittedExtensions, _fileSizeLimit, _targetFilePath, _directory, _file);
                if (image != null)
                {
                    images.Add(image);
                    _logger.LogInformation($"Saved new image to '{image.Path}'");
                }
                section = await requestReader.ReadNextSectionAsync();
            }
            await _images.SaveImages(images);
            return images;
            static async Task<ImageDTO> SaveSection(MultipartSection section, List<string> permittedExtensions, long fileSizeLimit, string targetFilePath, IDirectoryWrapper directory, IFileWrapper file)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                if (!hasContentDispositionHeader) return null;

                // This check assumes that there's a file
                // present without form data. If form data
                // is present, this method immediately fails
                // and returns the model error.
                if (!MultipartRequestHelper.HasFileContentDisposition(contentDisposition)) throw new InvalidDataException("File doesn't have content disposition");

                // Don't trust the file name sent by the client. To display
                var fileId = Guid.NewGuid();
                var trustedFileNameForFileStorage = $"original{Path.GetExtension(contentDisposition.FileName.Value)}";

                // **WARNING!**
                // In the following example, the file is saved without
                // scanning the file's contents. In most production
                // scenarios, an anti-virus/anti-malware scanner API
                // is used on the file before making the file available
                // for download or for use by other systems. 
                // For more information, see the topic that accompanies 
                // this sample.
                var streamedFileContent = await FileHelpers.ProcessStreamedFile(section, contentDisposition, permittedExtensions, fileSizeLimit);

                var fileFolder = $"{targetFilePath}\\{fileId}";

                directory.CreateDirectory(fileFolder);
                var filePath = $"{fileFolder}\\{trustedFileNameForFileStorage}";
                await using (var targetStream = file.Create(filePath))
                    await targetStream.WriteAsync(streamedFileContent);
                return new ImageDTO()
                {
                    Id = fileId,
                    Path = filePath
                };
            }
        }

        public async Task<int> DeleteImages(IEnumerable<Guid> imagesIds)
        {
            var images = await _images.GetImagesAsync(imagesIds);
            images.ForEach(i => _directory.Delete(Path.GetDirectoryName(i.Path), true));
            return await _images.DeleteImages(imagesIds);
        }

        public async Task<int> DeleteImage(Guid imageId)
        {
            var image = await _images.GetImageAsync(imageId);
            _directory.Delete(Path.GetDirectoryName(image.Path), true);
            return await _images.DeleteImage(imageId);
        }
    }
}
