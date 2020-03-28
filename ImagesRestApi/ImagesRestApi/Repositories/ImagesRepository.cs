using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ImagesRestApi.Databases.Images;
using ImagesRestApi.Databases.Images.Entities;
using ImagesRestApi.DTO;
using ImagesRestApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImagesRestApi.Repositories
{
    public class ImagesRepository : IImagesRepository
    {
        private readonly ImagesContext _context;
        private readonly ILogger<ImagesRepository> _logger;
        private readonly IMapper _mapper;

        private IQueryable<Image> _images;
        private IQueryable<Image> Images
        {
            get { return _images ??= _context.Images; }
        }
        public ImagesRepository(ImagesContext context, ILogger<ImagesRepository> logger, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ImageDTO> GetImageAsync(Guid id)
        {
            var dbImage = await Images.SingleOrDefaultAsync(i => i.Id == id);
            return _mapper.Map<ImageDTO>(dbImage);
        }
        public async Task<int> SaveImages(IEnumerable<ImageDTO> imagesDto)
        {
            var images = _mapper.Map<IEnumerable<Image>>(imagesDto);
            _context.AddRange(images);
            return await _context.SaveChangesAsync();
        }
        public async Task<int> DeleteImage(Guid imageId)
        {
            var image = await Images.SingleOrDefaultAsync(i => i.Id == imageId);
            if (image == null) return 0;
            _context.Remove(image);
            return await _context.SaveChangesAsync();
        }
        public async Task<int> DeleteImages(IEnumerable<Guid> imagesIds)
        {
            var images = await Images.Where(i => imagesIds.Contains(i.Id)).ToListAsync();
            if (!images.Any()) return 0;
            _context.RemoveRange(images);
            return await _context.SaveChangesAsync();
        }
    }
}
