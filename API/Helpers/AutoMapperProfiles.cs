using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        this.CreateMap<AppUser, MemberDto>()
            .ForMember(x => x.Age, y => y
                .MapFrom(z => z.DateOfBirth
                    .CalculateAge()))
            .ForMember(x => x.PhotoUrl, y=> y
                .MapFrom(z => z.Photos
                    .FirstOrDefault(x => x.IsMain)!.Url));
        this.CreateMap<Photo, PhotoDto>();
    }
}
