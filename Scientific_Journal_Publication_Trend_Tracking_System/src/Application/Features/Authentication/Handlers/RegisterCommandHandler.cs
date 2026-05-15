using MediatR;
using AutoMapper;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Authentication;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Constants;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Exceptions;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.Handlers;

/// <summary>
/// Handler for user registration command
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;
    private readonly ILogger<LoginCommandHandler> _logger;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IMapper mapper,
        ILogger<LoginCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing user registration for email: {Email}", request.Email);

        // Check if email already exists
        var emailExists = await _unitOfWork.Users.EmailExistsAsync(request.Email, cancellationToken);
        if (emailExists)
        {
            _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
            throw new ConflictException(ValidationMessages.EmailAlreadyExists);
        }

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLower(),
            FullName = request.FullName,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = UserRole.User,
            IsEmailVerified = false,
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User registered successfully: {UserId}", user.Id);

        return new RegisterResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Message = "User registered successfully"
        };
    }
}
