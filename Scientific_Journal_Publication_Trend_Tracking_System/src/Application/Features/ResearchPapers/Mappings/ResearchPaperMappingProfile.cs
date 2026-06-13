using AutoMapper;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Mappings;

/// <summary>
/// AutoMapper profile for ResearchPaper entity mappings
/// </summary>
public class ResearchPaperMappingProfile : Profile
{
    public ResearchPaperMappingProfile()
    {
        CreateMap<ResearchPaper, ResearchPaperDto>()
            .ForMember(dest => dest.Journal, opt => opt.MapFrom(src => src.Journal))
            .ForMember(dest => dest.Authors, opt => opt.MapFrom(src => src.Authors));

        CreateMap<Journal, JournalDto>();

        CreateMap<Author, AuthorDto>();
    }
}
