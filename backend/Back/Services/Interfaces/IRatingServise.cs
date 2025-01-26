using Back.Models;

namespace Back.Services.Interfaces
{
    public interface IRatingService
    {
        Task<bool> Calculate();
        Task<List<UserRating>> GetRatings(int limit, int offset);
        Task<int> GetRatingPosition(string username);
    }
}