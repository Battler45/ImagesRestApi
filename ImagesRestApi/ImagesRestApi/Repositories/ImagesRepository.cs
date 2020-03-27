using System;
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
    }
}
