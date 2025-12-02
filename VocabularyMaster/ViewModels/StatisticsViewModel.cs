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
    public class StatisticsViewModel : ViewModelBase
    {
        private readonly IWordRepository _wordRepository;
        private readonly IReviewHistoryRepository _reviewHistoryRepository;

        private int _totalWords;
        private int _reviewedWords;
        private int _totalReviews;
        private double _overallSuccessRate;
        private ObservableCollection<CategoryStatistic> _categoryStats;
        private ObservableCollection<DifficultyStatistic> _difficultyStats;

        public int TotalWords
        {
            get => _totalWords;
            set
            {
                if (SetProperty(ref _totalWords, value))
                {
                    OnPropertyChanged(nameof(NotReviewedWords));
                }
            }
        }

        public int ReviewedWords
        {
            get => _reviewedWords;
            set
            {
                if (SetProperty(ref _reviewedWords, value))
                {
                    OnPropertyChanged(nameof(NotReviewedWords));
                }
            }
        }

        public int NotReviewedWords => TotalWords - ReviewedWords;

        public int TotalReviews
        {
            get => _totalReviews;
            set => SetProperty(ref _totalReviews, value);
        }

        public double OverallSuccessRate
        {
            get => _overallSuccessRate;
            set => SetProperty(ref _overallSuccessRate, value);
        }

        public ObservableCollection<CategoryStatistic> CategoryStats
        {
            get => _categoryStats;
            set => SetProperty(ref _categoryStats, value);
        }

        public ObservableCollection<DifficultyStatistic> DifficultyStats
        {
            get => _difficultyStats;
            set => SetProperty(ref _difficultyStats, value);
        }

        public ICommand RefreshCommand { get; }

        public StatisticsViewModel(IWordRepository wordRepository, IReviewHistoryRepository reviewHistoryRepository)
        {
            _wordRepository = wordRepository;
            _reviewHistoryRepository = reviewHistoryRepository;
            _categoryStats = new ObservableCollection<CategoryStatistic>();
            _difficultyStats = new ObservableCollection<DifficultyStatistic>();

            RefreshCommand = new RelayCommand(async _ => await LoadStatisticsAsync());

            _ = LoadStatisticsAsync();
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                TotalWords = await _wordRepository.GetTotalWordCountAsync();
                ReviewedWords = await _wordRepository.GetReviewedWordCountAsync();
                OnPropertyChanged(nameof(NotReviewedWords));  // Bu zaten var

                TotalReviews = await _reviewHistoryRepository.GetTotalReviewCountAsync();
                OverallSuccessRate = await _reviewHistoryRepository.GetOverallSuccessRateAsync();

                await LoadCategoryStatisticsAsync();
                await LoadDifficultyStatisticsAsync();

                // TÜM PROPERTY'LERİ TEKRAR NOTIFY ET
                OnPropertyChanged(nameof(TotalWords));
                OnPropertyChanged(nameof(ReviewedWords));
                OnPropertyChanged(nameof(NotReviewedWords));
                OnPropertyChanged(nameof(TotalReviews));
                OnPropertyChanged(nameof(OverallSuccessRate));
                OnPropertyChanged(nameof(CategoryStats));
                OnPropertyChanged(nameof(DifficultyStats));
            }
            catch (Exception ex)
            {
                // Hata durumunda log
                System.Diagnostics.Debug.WriteLine($"İstatistik yükleme hatası: {ex.Message}");
            }
        }

        private async Task LoadCategoryStatisticsAsync()
        {
            var allWords = await _wordRepository.GetAllAsync();
            var grouped = allWords
                .Where(w => !string.IsNullOrEmpty(w.Category))
                .GroupBy(w => w.Category)
                .Select(g => new CategoryStatistic
                {
                    Category = g.Key!,
                    WordCount = g.Count(),
                    AverageSuccessRate = g.Average(w => w.SuccessRate)
                })
                .OrderByDescending(c => c.WordCount);

            CategoryStats.Clear();
            foreach (var stat in grouped)
            {
                CategoryStats.Add(stat);
            }
        }

        private async Task LoadDifficultyStatisticsAsync()
        {
            var allWords = await _wordRepository.GetAllAsync();
            var grouped = allWords
                .GroupBy(w => w.DifficultyLevel)
                .Select(g => new DifficultyStatistic
                {
                    Level = g.Key,
                    WordCount = g.Count(),
                    AverageSuccessRate = g.Average(w => w.SuccessRate)
                })
                .OrderBy(d => d.Level);

            DifficultyStats.Clear();
            foreach (var stat in grouped)
            {
                DifficultyStats.Add(stat);
            }
        }


    }

    public class CategoryStatistic
    {
        public string Category { get; set; } = string.Empty;
        public int WordCount { get; set; }
        public double AverageSuccessRate { get; set; }
    }

    public class DifficultyStatistic
    {
        public DifficultyLevel Level { get; set; }
        public int WordCount { get; set; }
        public double AverageSuccessRate { get; set; }
        public string LevelName => Level.ToString();
    }
}