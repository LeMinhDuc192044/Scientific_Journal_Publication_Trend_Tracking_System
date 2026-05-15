using AutoMapper;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.Commands;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.Mappings;

public class AuthenticationMappingProfile : Profile
{
    public AuthenticationMappingProfile()
    {
        CreateMap<RegisterRequest, RegisterCommand>();
        CreateMap<LoginRequest, LoginCommand>();
        CreateMap<RefreshTokenRequest, RefreshTokenCommand>();

        CreateMap<User, RegisterResponse>();
        CreateMap<User, LoginResponse>();
    }
}
