using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VocabularyMaster.Core.Models;
using VocabularyMaster.Core.Enums; // YENİ

namespace VocabularyMaster.Core.Interfaces
{
    public interface IWordRepository
    {
        Task<Word> AddAsync(Word word);
        Task<Word?> GetByIdAsync(int id);
        Task<List<Word>> GetAllAsync();
        Task UpdateAsync(Word word);
        Task DeleteAsync(int id);
        Task<List<Word>> GetByCategoryAsync(string category);
        Task<List<Word>> GetByDifficultyAsync(DifficultyLevel level);
        Task<List<string>> GetAllCategoriesAsync();
        Task<Word?> GetRandomWordAsync();
        Task<List<Word>> GetRandomWordsAsync(int count);
        Task<int> GetTotalWordCountAsync();
        Task<int> GetReviewedWordCountAsync();
        Task<List<Word>> GetWordsNeedingReviewAsync();
        Task<List<Word>> GetFavoriteWordsAsync();
        Task<Word?> GetRandomFavoriteWordAsync();
        Task<List<Word>> GetRecentWordsAsync(int count);
        Task<int> GetTodayAddedCountAsync();
        Task<int> GetThisWeekAddedCountAsync();
    }
}