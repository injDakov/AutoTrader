using AT.Business.Models.Dto;
using AT.Domain;
using AutoMapper;

namespace AT.Business.AutoMapper
{
    public class DtoToEntity : Profile
    {
        public DtoToEntity()
        {
            CreateMap<LogDto, Log>()
                .ForMember(dest => dest.Message, opt => opt.MapFrom(s => s.DetailedMessage.Text))
                .ForMember(dest => dest.DetailedMessage, opt => opt.MapFrom(s => string.Empty));

            // TODO
            //.ForMember(dest => dest.DetailedMessage, opt => opt.MapFrom(s => JsonSerializer.Serialize(s.DetailedMessage)));
        }
    }
}