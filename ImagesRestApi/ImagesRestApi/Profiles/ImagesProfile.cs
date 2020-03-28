using AutoMapper;
using ImagesRestApi.DTO;

namespace ImagesRestApi.Profiles
{
    public class ImagesProfile : Profile
    {
        public ImagesProfile()
        {
            CreateMap<Databases.Images.Entities.Image, ImageDTO>()
                .ReverseMap();
        }
    }
}
