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

namespace ImagesRestApi.Repositories
{
    public class ImagesRepository : IImagesRepository
    {
        private readonly ImagesContext _context;
        private readonly IMapper _mapper;

        private IQueryable<Image> _images;
        private IQueryable<Image> Images
        {
            get { return _images ??= _context.Images; }
        }
        public ImagesRepository(ImagesContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ImageDTO> GetImageAsync(Guid id)
        {
            var dbImage = await Images.AsNoTracking().SingleOrDefaultAsync(i => i.Id == id);
            return _mapper.Map<ImageDTO>(dbImage);
        }
        public async Task<List<ImageDTO>> GetImagesAsync(IEnumerable<Guid> ids)
        {
            var dbImages = await Images.AsNoTracking().Where(i => ids.Contains(i.Id)).ToListAsync();
            return _mapper.Map<List<ImageDTO>>(dbImages);
        }
        public async Task<int> SaveImages(IEnumerable<ImageDTO> imagesDto)
        {
            var images = _mapper.Map<IEnumerable<Image>>(imagesDto);
            await _context.AddRangeAsync(images);
            return await _context.SaveChangesAsync();
        }
        public async Task<int> SaveImage(ImageDTO imageDto)
        {
            var image = _mapper.Map<Image>(imageDto);
            await _context.AddAsync(image);
            return await _context.SaveChangesAsync();
        }
        public async Task<int> UpdateImages(IEnumerable<ImageDTO> imagesDto)
        {
            var ids = imagesDto.Select(i => i.Id).ToList();
            var dbImages = await Images.Where(i => ids.Contains(i.Id)).ToListAsync();
            _mapper.Map(imagesDto, dbImages);
            return await _context.SaveChangesAsync();
        }
        public async Task<int> UpdateImage(ImageDTO imageDto)
        {
            var dbImage = await Images.Where(i => i.Id == imageDto.Id).SingleOrDefaultAsync();
            _mapper.Map(imageDto, dbImage);
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
