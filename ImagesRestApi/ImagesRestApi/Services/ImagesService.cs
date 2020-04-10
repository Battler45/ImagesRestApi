using ImagesRestApi.Models;
using ImagesRestApi.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using ImagesRestApi.DTO;
using ImagesRestApi.Services.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using ImagesRestApi.Utilities;
using ImagesRestApi.Wrappers;
using Microsoft.AspNetCore.StaticFiles;
using File = ImagesRestApi.Models.File;

namespace ImagesRestApi.Services
{
    public class ImagesService : IImagesService
    {
        private readonly IImagesRepository _images;

        //wrappers
        private readonly IContentDispositionHeaderValueWrapper _contentDispositionHeaderValue;
        private readonly IContentTypeProvider _contentTypeProvider;
        private readonly IImagesStorageService _imagesStorage;

        private readonly List<string> _permittedExtensions;
        private readonly long _fileSizeLimit;

        public ImagesService(IImagesRepository images, IConfiguration config, IFileWrapper file, IPathWrapper path, 
            IContentDispositionHeaderValueWrapper contentDispositionHeaderValue, IContentTypeProvider contentTypeProvider,
            IImagesStorageService imagesStorage)
        {
            _images = images ?? throw new ArgumentNullException(nameof(images)); 
            _contentDispositionHeaderValue = contentDispositionHeaderValue ?? throw new ArgumentNullException(nameof(contentDispositionHeaderValue));
            _contentTypeProvider = contentTypeProvider ?? throw new ArgumentNullException(nameof(contentTypeProvider));
            _imagesStorage = imagesStorage ?? throw new ArgumentNullException(nameof(imagesStorage));

            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");

            _permittedExtensions = config.GetSection("PermittedExtensions").AsEnumerable()
                .Where(p => p.Value != null)
                .Select(p => p.Value)
                .ToList();
        }

        #region Get
        public async Task<Image> GetImageAsync(Guid id)
        {
            var imageDto = await _images.GetImageAsync(id);

            var imageFile = await _imagesStorage.GetImage(imageDto);
            if (imageFile == null) return null;
            _contentTypeProvider.TryGetContentType(imageDto.Path, out var contentType);
            return new Image()
            {
                Id = imageDto.Id,
                Content = imageFile,
                ContentType = contentType
            };
        }
        #endregion

        #region Save
        public async Task<ImageDTO> SaveImage(PipeReader reader, string contentType)
        {
            // **WARNING!**
            // In the following example, the file is saved without
            // scanning the file's contents. In most production
            // scenarios, an anti-virus/anti-malware scanner API
            // is used on the file before making the file available
            // for download or for use by other systems. 
            // For more information, see the topic that accompanies 
            // this sample.
            var streamedFile = await FileHelpers.ProcessStreamedFile(reader, contentType, _permittedExtensions, _fileSizeLimit, _contentTypeProvider);
            var image = await _imagesStorage.SaveImage(streamedFile, Guid.NewGuid());
            await _images.SaveImage(image);
            return image;
        }

        public async Task<ImageDTO> SaveImage(File file)
        {
            // **WARNING!**
            // In the following example, the file is saved without
            // scanning the file's contents. In most production
            // scenarios, an anti-virus/anti-malware scanner API
            // is used on the file before making the file available
            // for download or for use by other systems. 
            // For more information, see the topic that accompanies 
            // this sample.
            var streamedFile = FileHelpers.ProcessFile(file, _permittedExtensions, _fileSizeLimit);
            var image = await _imagesStorage.SaveImage(streamedFile, Guid.NewGuid());
            await _images.SaveImage(image);
            return image;
        }
        public async Task<List<ImageDTO>> SaveImages(IEnumerable<File> files)
        {
            // **WARNING!**
            // In the following example, the file is saved without
            // scanning the file's contents. In most production
            // scenarios, an anti-virus/anti-malware scanner API
            // is used on the file before making the file available
            // for download or for use by other systems. 
            // For more information, see the topic that accompanies 
            // this sample.
            var processedFiles = files.Select(f => FileHelpers.ProcessFile(f, _permittedExtensions, _fileSizeLimit));
            var saveImages = processedFiles.Select(pf => _imagesStorage.SaveImage(pf, Guid.NewGuid()));
            var images = await Task.WhenAll(saveImages);
            await _images.SaveImages(images);
            return images.ToList();
        }

        public async Task<List<ImageDTO>> SaveImages(MultipartReader requestReader)
        {
            var images = new List<ImageDTO>();
            var section = await requestReader.ReadNextSectionAsync();
            while (section != null)
            {
                var image = await SaveSection(section, _permittedExtensions, _fileSizeLimit, _contentDispositionHeaderValue);
                if (image != null) images.Add(image);
                section = await requestReader.ReadNextSectionAsync();
            }
            await _images.SaveImages(images);
            return images;
            async Task<ImageDTO> SaveSection(MultipartSection section, List<string> permittedExtensions, long fileSizeLimit,
                IContentDispositionHeaderValueWrapper contentDispositionHeaderValue)
            {
                var hasContentDispositionHeader = contentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                if (!hasContentDispositionHeader) return null;

                // This check assumes that there's a file
                // present without form data. If form data
                // is present, this method immediately fails
                // and returns the model error.
                if (!MultipartRequestHelper.HasFileContentDisposition(contentDisposition)) throw new InvalidDataException("ProcessedStreamedFile doesn't have content disposition");
                // **WARNING!**
                // In the following example, the file is saved without
                // scanning the file's contents. In most production
                // scenarios, an anti-virus/anti-malware scanner API
                // is used on the file before making the file available
                // for download or for use by other systems. 
                // For more information, see the topic that accompanies 
                // this sample.
                var streamedFile = await FileHelpers.ProcessStreamedFile(section, contentDisposition, permittedExtensions, fileSizeLimit);
                var imageId = Guid.NewGuid();
                return await _imagesStorage.SaveImage(streamedFile, imageId);
            }
        }
        #endregion

        #region Delete

        public async Task<int> DeleteImages(IEnumerable<Guid> imagesIds)
        {
            var images = await _images.GetImagesAsync(imagesIds);
            images.ForEach(_imagesStorage.DeleteImageDirectory);
            return await _images.DeleteImages(imagesIds);
        }

        public async Task<int> DeleteImage(Guid imageId)
        {
            var image = await _images.GetImageAsync(imageId);
            _imagesStorage.DeleteImageDirectory(image);
            return await _images.DeleteImage(imageId);
        }
        #endregion

        #region Update
        public async Task<List<ImageDTO>> UpdateImages(IEnumerable<Image> files)
        {
            var images = await _images.GetImagesAsync(files.Select(i => i.Id));
            images.ForEach(_imagesStorage.DeleteImageDirectory);
            // **WARNING!**
            // In the following example, the file is saved without
            // scanning the file's contents. In most production
            // scenarios, an anti-virus/anti-malware scanner API
            // is used on the file before making the file available
            // for download or for use by other systems. 
            // For more information, see the topic that accompanies 
            // this sample.
            //TODO: create update method to not use repository(not changed path)
            var saveImages = files.Select(f => _imagesStorage.SaveImage(FileHelpers.ProcessFile(f, _permittedExtensions, _fileSizeLimit), f.Id));
            images = (await Task.WhenAll(saveImages)).ToList();
            await _images.UpdateImages(images);
            return images;
        }

        public async Task<ImageDTO> UpdateImage(Image file) => (await UpdateImages(new List<Image>() {file})).First();

        public async Task<ImageDTO> UpdateImage(PipeReader reader, Guid imageId, string contentType)
        {
            var image = await _images.GetImageAsync(imageId);
            _imagesStorage.DeleteImageDirectory(image);
            // **WARNING!**
            // In the following example, the file is saved without
            // scanning the file's contents. In most production
            // scenarios, an anti-virus/anti-malware scanner API
            // is used on the file before making the file available
            // for download or for use by other systems. 
            // For more information, see the topic that accompanies 
            // this sample.
            //TODO: create update method to not use repository(not changed path)
            //TODO: create update method to ProcessStreamedFile to not use _contentTypeProvider
            var streamedFile = await FileHelpers.ProcessStreamedFile(reader, contentType, _permittedExtensions, _fileSizeLimit, _contentTypeProvider);
            image = await _imagesStorage.SaveImage(streamedFile, imageId);
            await _images.UpdateImage(image);
            return image;
        }

        public async Task<ImageDTO> UpdateImage(MultipartReader requestReader, Guid imageId)
        {
            ProcessedStreamedFile image = null;
            var section = await requestReader.ReadNextSectionAsync();
            while (section != null && image == null)
            {
                image = await GetImage(section, _permittedExtensions, _fileSizeLimit, _contentDispositionHeaderValue);
                section = await requestReader.ReadNextSectionAsync();
            }
            //TODO: create update method to not use repository(not changed path)
            var imageDto = await _imagesStorage.SaveImage(image, imageId);
            await _images.UpdateImage(imageDto);
            return imageDto;
            async Task<ProcessedStreamedFile> GetImage(MultipartSection section, List<string> permittedExtensions, long fileSizeLimit,
                IContentDispositionHeaderValueWrapper contentDispositionHeaderValue)
            {
                var hasContentDispositionHeader = contentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                if (!hasContentDispositionHeader) return null;

                // This check assumes that there's a file
                // present without form data. If form data
                // is present, this method immediately fails
                // and returns the model error.
                if (!MultipartRequestHelper.HasFileContentDisposition(contentDisposition)) throw new InvalidDataException("ProcessedStreamedFile doesn't have content disposition");
                // **WARNING!**
                // In the following example, the file is saved without
                // scanning the file's contents. In most production
                // scenarios, an anti-virus/anti-malware scanner API
                // is used on the file before making the file available
                // for download or for use by other systems. 
                // For more information, see the topic that accompanies 
                // this sample.
                var streamedFile = await FileHelpers.ProcessStreamedFile(section, contentDisposition, permittedExtensions, fileSizeLimit);
                // Don't trust the file name sent by the client. To display
                return streamedFile;
            }
        }
        #endregion
    }
}
