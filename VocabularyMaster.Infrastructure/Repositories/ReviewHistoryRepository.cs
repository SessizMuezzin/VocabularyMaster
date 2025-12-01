using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VocabularyMaster.Core.Interfaces;
using VocabularyMaster.Core.Models;
using VocabularyMaster.Core.Enums; // YENİ
using VocabularyMaster.Infrastructure.Data;

namespace VocabularyMaster.Infrastructure.Repositories
{
    public class ReviewHistoryRepository : IReviewHistoryRepository
    {
        private readonly VocabularyDbContext _context;

        public ReviewHistoryRepository(VocabularyDbContext context)
        {
            _context = context;
        }

        public async Task<ReviewHistory> AddAsync(ReviewHistory review)
        {
            review.ReviewDate = DateTime.Now;
            _context.ReviewHistories.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<List<ReviewHistory>> GetByWordIdAsync(int wordId)
        {
            return await _context.ReviewHistories
                .Where(r => r.WordId == wordId)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();
        }

        public async Task<List<ReviewHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.ReviewHistories
                .Where(r => r.ReviewDate >= startDate && r.ReviewDate <= endDate)
                .ToListAsync();
        }

        public async Task<int> GetTotalReviewCountAsync()
        {
            return await _context.ReviewHistories.CountAsync();
        }

        public async Task<double> GetOverallSuccessRateAsync()
        {
            var totalReviews = await _context.ReviewHistories.CountAsync();
            if (totalReviews == 0)
                return 0;

            var correctReviews = await _context.ReviewHistories.CountAsync(r => r.IsCorrect);
            return (double)correctReviews / totalReviews * 100;
        }

        public async Task<Dictionary<DateTime, int>> GetDailyReviewCountsAsync(int days)
        {
            var startDate = DateTime.Now.Date.AddDays(-days);
            var reviews = await _context.ReviewHistories
                .Where(r => r.ReviewDate >= startDate)
                .ToListAsync();

            return reviews
                .GroupBy(r => r.ReviewDate.Date)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}