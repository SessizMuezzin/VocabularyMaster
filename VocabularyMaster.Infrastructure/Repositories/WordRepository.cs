using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VocabularyMaster.Core.Interfaces;
using VocabularyMaster.Core.Models;
using VocabularyMaster.Core.Enums;
using VocabularyMaster.Infrastructure.Data;

namespace VocabularyMaster.Infrastructure.Repositories
{
    public class WordRepository : IWordRepository
    {
        private readonly VocabularyDbContext _context;

        public WordRepository(VocabularyDbContext context)
        {
            _context = context;
        }

        public async Task<Word> AddAsync(Word word)
        {
            word.DateAdded = DateTime.Now;
            _context.Words.Add(word);
            await _context.SaveChangesAsync();
            return word;
        }

        public async Task<Word?> GetByIdAsync(int id)
        {
            return await _context.Words
                .Include(w => w.Meanings)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<List<Word>> GetAllAsync()
        {
            return await _context.Words
                .Include(w => w.Meanings)
                .AsNoTracking()
                .AsSplitQuery()
                .OrderByDescending(w => w.DateAdded)
                .ToListAsync();
        }

        public async Task UpdateAsync(Word word)
        {
            _context.Words.Update(word);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var word = await _context.Words.FindAsync(id);
            if (word != null)
            {
                _context.Words.Remove(word);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Word>> GetByCategoryAsync(string category)
        {
            return await _context.Words
                .Include(w => w.Meanings)
                .Where(w => w.Category == category)
                .ToListAsync();
        }

        public async Task<List<Word>> GetByDifficultyAsync(DifficultyLevel level)
        {
            return await _context.Words
                .Include(w => w.Meanings)
                .Where(w => w.DifficultyLevel == level)
                .ToListAsync();
        }

        public async Task<List<string>> GetAllCategoriesAsync()
        {
            return await _context.Words
                .Where(w => !string.IsNullOrEmpty(w.Category))
                .Select(w => w.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<Word?> GetRandomWordAsync()
        {
            var words = await _context.Words
                .Include(w => w.Meanings)
                .ToListAsync();

            if (!words.Any())
                return null;

            var random = new Random();
            return words[random.Next(words.Count)];
        }

        public async Task<List<Word>> GetRandomWordsAsync(int count)
        {
            var words = await _context.Words.ToListAsync();
            if (!words.Any())
                return new List<Word>();

            var random = new Random();
            return words.OrderBy(x => random.Next()).Take(count).ToList();
        }

        public async Task<int> GetTotalWordCountAsync()
        {
            return await _context.Words.CountAsync();
        }

        public async Task<int> GetReviewedWordCountAsync()
        {
            return await _context.Words
                .Where(w => w.ReviewCount > 0)
                .CountAsync();
        }


        public async Task<List<Word>> GetWordsNeedingReviewAsync()
        {
            var cutoffDate = DateTime.Now.AddDays(-7);
            return await _context.Words
                .Where(w => w.LastReviewed == null || w.LastReviewed < cutoffDate)
                .OrderBy(w => w.LastReviewed)
                .ToListAsync();
        }

        public async Task<List<Word>> GetFavoriteWordsAsync()
        {
            return await _context.Words
                .Include(w => w.Meanings)
                .Where(w => w.IsFavorite)
                .OrderByDescending(w => w.DateAdded)
                .ToListAsync();
        }

        public async Task<Word?> GetRandomFavoriteWordAsync()
        {
            var favorites = await _context.Words.Where(w => w.IsFavorite).ToListAsync();
            if (!favorites.Any())
                return null;

            var random = new Random();
            return favorites[random.Next(favorites.Count)];
        }

        public async Task<List<Word>> GetRecentWordsAsync(int count)
        {
            return await _context.Words
                .Include(w => w.Meanings)
                .OrderByDescending(w => w.DateAdded)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetTodayAddedCountAsync()
        {
            var today = DateTime.Today;
            return await _context.Words
                .CountAsync(w => w.DateAdded.Date == today);
        }

        public async Task<int> GetThisWeekAddedCountAsync()
        {
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            return await _context.Words
                .CountAsync(w => w.DateAdded >= startOfWeek);
        }
    }
}