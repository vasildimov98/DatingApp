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
        this.CreateMap<MemberUpdateDto, AppUser>();
        this.CreateMap<RegisterDto, AppUser>();
        this.CreateMap<string, DateOnly>().ConvertUsing(x => DateOnly.Parse(x));
        CreateMap<Message, MessageDto>()
            .ForMember(x => x.SenderPhotoUrl,
                y => y.MapFrom(x => x.Sender.Photos.FirstOrDefault(x => x.IsMain)!.Url))
            .ForMember(x => x.RecipientPhotoUrl,
                y => y.MapFrom(x => x.Recipient.Photos.FirstOrDefault(x => x.IsMain)!.Url));
        CreateMap<Photo, PhotoForApprovalDto>()
            .ForMember(x => x.Username, y => y.MapFrom(z => z.AppUser.UserName));

        CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
        CreateMap<DateTime?, DateTime?>().ConstructUsing(d => d.HasValue ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null);
    }
}
