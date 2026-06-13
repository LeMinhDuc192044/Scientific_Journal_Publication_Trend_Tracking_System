using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Infrastructure.BackgroundJobs
{
    public class RefreshTokenHistoryJob
    {
        private readonly IUnitOfWork _unitOfWork;

        public RefreshTokenHistoryJob(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LogTokenAsync(
            Guid userId,
            string token,
            DateTime expiry)
        {
            await _unitOfWork.RefreshTokenHistories.AddAsync(
                new RefreshTokenHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Token = token,
                    ExpiryTime = expiry,
                    IsRevoked = false
                });

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
