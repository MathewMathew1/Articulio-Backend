using AutoMapper;
using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Mapping
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            CreateMap<User, UserDto>();
        }
    }
}
