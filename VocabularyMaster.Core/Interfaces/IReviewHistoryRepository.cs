using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VocabularyMaster.Core.Models;

namespace VocabularyMaster.Core.Interfaces
{
    public interface IReviewHistoryRepository
    {
        Task<ReviewHistory> AddAsync(ReviewHistory review);
        Task<List<ReviewHistory>> GetByWordIdAsync(int wordId);
        Task<List<ReviewHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetTotalReviewCountAsync();
        Task<double> GetOverallSuccessRateAsync();
        Task<Dictionary<DateTime, int>> GetDailyReviewCountsAsync(int days);
    }
}