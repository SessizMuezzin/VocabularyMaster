using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using VocabularyMaster.Core.Interfaces;
using VocabularyMaster.Core.Models;
using VocabularyMaster.Core.Enums;
using VocabularyMaster.WPF.Commands;

namespace VocabularyMaster.WPF.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly IWordRepository _wordRepository;
        private readonly IReviewHistoryRepository _reviewHistoryRepository;

        private int _totalWords;
        private int _todayAdded;
        private int _weekAdded;
        private int _favoriteCount;
        private int _todayReviews;
        private int _currentStreak;
        private ObservableCollection<Word> _recentWords;

        public int TotalWords
        {
            get => _totalWords;
            set => SetProperty(ref _totalWords, value);
        }

        public int TodayAdded
        {
            get => _todayAdded;
            set => SetProperty(ref _todayAdded, value);
        }

        public int WeekAdded
        {
            get => _weekAdded;
            set => SetProperty(ref _weekAdded, value);
        }

        public int FavoriteCount
        {
            get => _favoriteCount;
            set => SetProperty(ref _favoriteCount, value);
        }

        public int TodayReviews
        {
            get => _todayReviews;
            set => SetProperty(ref _todayReviews, value);
        }

        public int CurrentStreak
        {
            get => _currentStreak;
            set => SetProperty(ref _currentStreak, value);
        }

        public ObservableCollection<Word> RecentWords
        {
            get => _recentWords;
            set => SetProperty(ref _recentWords, value);
        }

        public ICommand RefreshCommand { get; }

        public DashboardViewModel(IWordRepository wordRepository, IReviewHistoryRepository reviewHistoryRepository)
        {
            _wordRepository = wordRepository;
            _reviewHistoryRepository = reviewHistoryRepository;
            _recentWords = new ObservableCollection<Word>();

            RefreshCommand = new RelayCommand(async _ => await LoadDashboardAsync());

            _ = LoadDashboardAsync();
        }

        private async Task LoadDashboardAsync()
        {
            TotalWords = await _wordRepository.GetTotalWordCountAsync();
            TodayAdded = await _wordRepository.GetTodayAddedCountAsync();
            WeekAdded = await _wordRepository.GetThisWeekAddedCountAsync();

            var favorites = await _wordRepository.GetFavoriteWordsAsync();
            FavoriteCount = favorites.Count;

            // Bugünkü testler
            var today = DateTime.Today;
            var todayReviews = await _reviewHistoryRepository.GetByDateRangeAsync(today, DateTime.Now);
            TodayReviews = todayReviews.Count;

            // Streak hesapla (basit versiyon)
            CurrentStreak = await CalculateStreakAsync();

            // Son eklenen kelimeler
            var recent = await _wordRepository.GetRecentWordsAsync(10);
            RecentWords.Clear();
            foreach (var word in recent)
            {
                RecentWords.Add(word);
            }
        }

        private async Task<int> CalculateStreakAsync()
        {
            var streak = 0;
            var date = DateTime.Today;

            for (int i = 0; i < 365; i++)
            {
                var reviews = await _reviewHistoryRepository.GetByDateRangeAsync(date, date.AddDays(1));
                if (reviews.Count > 0)
                {
                    streak++;
                    date = date.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return streak;
        }
    }
}