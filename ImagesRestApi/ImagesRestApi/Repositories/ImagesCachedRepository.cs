using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ImagesRestApi.Databases.Images;
using ImagesRestApi.Repositories.Interfaces;

namespace ImagesRestApi.Repositories
{
    //TODO:
    public class ImagesCachedRepository: ImagesRepository, IImagesRepository
    {
        public ImagesCachedRepository(ImagesContext context, IMapper mapper): base(context, mapper)
        {
        }
    }
}
