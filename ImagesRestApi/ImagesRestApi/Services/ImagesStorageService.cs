using System;
using System.Threading.Tasks;
using ImagesRestApi.DTO;
using ImagesRestApi.Models;
using ImagesRestApi.Services.Interfaces;
using ImagesRestApi.Wrappers;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;

namespace ImagesRestApi.Services
{
    public class ImagesStorageService : IImagesStorageService
    {
        //wrappers
        private readonly IDirectoryWrapper _directory;
        private readonly IFileWrapper _file;
        private readonly IPathWrapper _path;

        private readonly string _targetFileStoragePath;


        public ImagesStorageService(IConfiguration config, IDirectoryWrapper directory, IFileWrapper file, IPathWrapper path,
            IContentDispositionHeaderValueWrapper contentDispositionHeaderValue, IContentTypeProvider contentTypeProvider)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            _file = file ?? throw new ArgumentNullException(nameof(file));
            _path = path ?? throw new ArgumentNullException(nameof(path));

            _targetFileStoragePath = config.GetValue<string>("StoredFilesPath");
        }

        public async Task<ImageDTO> SaveImage(ProcessedStreamedFile file, Guid fileId)
        {
            var folderName = Guid.NewGuid().ToString();// Don't trust the file name sent by the client. To display
            var fileFolder = _path.Combine(_targetFileStoragePath, folderName); //var fileFolder = $"{_targetFileStoragePath}\\{fileId}";
            _directory.CreateDirectory(fileFolder);

            var trustedFileNameForFileStorage = $"original{file.Extension}";
            var filePath = _path.Combine(fileFolder, trustedFileNameForFileStorage);//$"{fileFolder}\\{trustedFileNameForFileStorage}";
            await using (var targetStream = _file.Create(filePath))
                await targetStream.WriteAsync(file.Content);
            return new ImageDTO()
            {
                Id = fileId,
                Path = filePath
            };
        }

        public async Task<byte[]> GetImage(ImageDTO image) => image == null || !_file.Exists(image.Path) ? null : await _file.ReadAllBytesAsync(image.Path);
        public void DeleteImageDirectory(ImageDTO image) => _directory.Delete(_path.GetDirectoryName(image.Path), true);
    }
}
