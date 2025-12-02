using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VocabularyMaster.Core.Interfaces;
using VocabularyMaster.Core.Models;
using VocabularyMaster.Core.Enums;
using VocabularyMaster.WPF.Commands;

namespace VocabularyMaster.WPF.ViewModels
{
    public class FlashcardViewModel : ViewModelBase
    {
        private readonly IWordRepository _wordRepository;
        private readonly IReviewHistoryRepository _reviewHistoryRepository;
        private List<Word> _originalFilteredWords = new();
        private List<Word> _allWords = new();
        private List<Word> _filteredWords = new();
        private int _currentIndex = 0;

        public FlashcardViewModel(IWordRepository wordRepository, IReviewHistoryRepository reviewHistoryRepository)
        {
            _wordRepository = wordRepository;
            _reviewHistoryRepository = reviewHistoryRepository;

            FlipCardCommand = new RelayCommand(_ => FlipCard());
            NextCardCommand = new RelayCommand(_ => NextCard(), _ => CanGoNext());
            PreviousCardCommand = new RelayCommand(_ => PreviousCard(), _ => CanGoPrevious());
            MarkCorrectCommand = new RelayCommand(async _ => await MarkAnswerAsync(true));
            MarkWrongCommand = new RelayCommand(async _ => await MarkAnswerAsync(false));
            ShuffleCommand = new RelayCommand(_ => ShuffleCards());
            RestartCommand = new RelayCommand(_ => RestartSession());
            ApplyFilterCommand = new RelayCommand(_ => ApplyFilter());

            _ = LoadWordsAsync();
        }

        #region Properties

        private Word? _currentWord;
        public Word? CurrentWord
        {
            get => _currentWord;
            set
            {
                _currentWord = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EnglishWord));
                OnPropertyChanged(nameof(TurkishMeanings));
                OnPropertyChanged(nameof(ExampleSentence));
                OnPropertyChanged(nameof(ProgressText));
                OnPropertyChanged(nameof(HasWords));
                OnPropertyChanged(nameof(DifficultyColor));
                OnPropertyChanged(nameof(DifficultyText));
            }
        }

        private bool _isFlipped;
        public bool IsFlipped
        {
            get => _isFlipped;
            set
            {
                _isFlipped = value;
                OnPropertyChanged();
            }
        }

        private bool _showOnlyFavorites;
        public bool ShowOnlyFavorites
        {
            get => _showOnlyFavorites;
            set
            {
                _showOnlyFavorites = value;
                OnPropertyChanged();
            }
        }

        private bool _isShuffled;
        public bool IsShuffled
        {
            get => _isShuffled;
            set
            {
                _isShuffled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShuffleButtonText));
                OnPropertyChanged(nameof(ShuffleButtonColor));
            }
        }

        public string ShuffleButtonText => IsShuffled ? "↩️ Sırala" : "🔀 Karıştır";
        public string ShuffleButtonColor => IsShuffled ? "#10b981" : "#64748b";

        private string? _selectedDifficultyString = "Tümü";
        public string? SelectedDifficultyString
        {
            get => _selectedDifficultyString;
            set
            {
                _selectedDifficultyString = value;
                OnPropertyChanged();
            }
        }

        public string EnglishWord => CurrentWord?.English ?? "Kelime yok";

        public string TurkishMeanings
        {
            get
            {
                if (CurrentWord == null) return "";

                if (CurrentWord.Meanings?.Any() == true)
                {
                    return string.Join("\n", CurrentWord.Meanings
                        .OrderBy(m => m.DisplayOrder)
                        .Select((m, i) => $"{i + 1}. {m.Turkish}"));
                }

                return CurrentWord.Turkish;
            }
        }

        public string ExampleSentence
        {
            get
            {
                if (CurrentWord == null) return "";

                if (CurrentWord.Meanings?.Any() == true)
                {
                    var exampleMeaning = CurrentWord.Meanings
                        .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m.ExampleSentence));
                    return exampleMeaning?.ExampleSentence ?? "";
                }

                return CurrentWord.ExampleSentence ?? "";
            }
        }

        public string ProgressText => HasWords ? $"{_currentIndex + 1} / {_filteredWords.Count}" : "0 / 0";

        public bool HasWords => _filteredWords.Any();

        public string DifficultyColor
        {
            get
            {
                if (CurrentWord == null) return "#95a5a6";
                return CurrentWord.DifficultyLevel switch
                {
                    DifficultyLevel.A1 => "#27ae60",
                    DifficultyLevel.A2 => "#2ecc71",
                    DifficultyLevel.B1 => "#3498db",
                    DifficultyLevel.B2 => "#9b59b6",
                    DifficultyLevel.C1 => "#e67e22",
                    DifficultyLevel.C2 => "#e74c3c",
                    _ => "#95a5a6"
                };
            }
        }

        public string DifficultyText
        {
            get
            {
                if (CurrentWord == null) return "";
                return CurrentWord.DifficultyLevel.ToString();
            }
        }

        private int _correctCount;
        public int CorrectCount
        {
            get => _correctCount;
            set
            {
                _correctCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SessionStats));
            }
        }

        private int _wrongCount;
        public int WrongCount
        {
            get => _wrongCount;
            set
            {
                _wrongCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SessionStats));
            }
        }

        public string SessionStats => $"✅ {CorrectCount}  ❌ {WrongCount}  📊 {SuccessRate:F1}%";

        public double SuccessRate
        {
            get
            {
                var total = CorrectCount + WrongCount;
                return total > 0 ? (double)CorrectCount / total * 100 : 0;
            }
        }

        public int TotalWordsCount => _allWords.Count;
        public int FilteredWordsCount => _filteredWords.Count;

        #endregion

        #region Commands

        public ICommand FlipCardCommand { get; }
        public ICommand NextCardCommand { get; }
        public ICommand PreviousCardCommand { get; }
        public ICommand MarkCorrectCommand { get; }
        public ICommand MarkWrongCommand { get; }
        public ICommand ShuffleCommand { get; }
        public ICommand RestartCommand { get; }
        public ICommand ApplyFilterCommand { get; }

        #endregion

        #region Methods

        private async Task LoadWordsAsync()
        {
            try
            {
                var words = await _wordRepository.GetAllAsync();
                _allWords = words.ToList();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kelimeler yüklenirken hata oluştu: {ex.Message}",
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ApplyFilter()
        {
            _filteredWords = _allWords.ToList();

            // Favori filtresi
            if (ShowOnlyFavorites)
            {
                _filteredWords = _filteredWords.Where(w => w.IsFavorite).ToList();
            }

            // Seviye filtresi
            if (!string.IsNullOrEmpty(SelectedDifficultyString) && SelectedDifficultyString != "Tümü")
            {
                if (Enum.TryParse<DifficultyLevel>(SelectedDifficultyString, out var level))
                {
                    _filteredWords = _filteredWords.Where(w => w.DifficultyLevel == level).ToList();
                }
            }

            // ORİJİNAL SIRAYI SAKLA
            _originalFilteredWords = _filteredWords.ToList();

            // İstatistikleri sıfırla
            CorrectCount = 0;
            WrongCount = 0;
            IsShuffled = false;

            // İlk karta git
            if (_filteredWords.Any())
            {
                _currentIndex = 0;
                CurrentWord = _filteredWords[_currentIndex];
                IsFlipped = false;
            }
            else
            {
                CurrentWord = null;
            }

            OnPropertyChanged(nameof(HasWords));
            OnPropertyChanged(nameof(ProgressText));
            OnPropertyChanged(nameof(FilteredWordsCount));
        }

        private void FlipCard()
        {
            IsFlipped = !IsFlipped;
        }

        private void NextCard()
        {
            if (CanGoNext())
            {
                _currentIndex++;
                CurrentWord = _filteredWords[_currentIndex];
                IsFlipped = false;
            }
        }

        private bool CanGoNext()
        {
            return _filteredWords.Any() && _currentIndex < _filteredWords.Count - 1;
        }

        private void PreviousCard()
        {
            if (CanGoPrevious())
            {
                _currentIndex--;
                CurrentWord = _filteredWords[_currentIndex];
                IsFlipped = false;
            }
        }

        private bool CanGoPrevious()
        {
            return _filteredWords.Any() && _currentIndex > 0;
        }

        private async Task MarkAnswerAsync(bool isCorrect)
        {
            if (CurrentWord == null) return;

            try
            {
                // İstatistikleri güncelle
                if (isCorrect)
                    CorrectCount++;
                else
                    WrongCount++;

                // Veritabanını güncelle
                var reviewHistory = new ReviewHistory
                {
                    WordId = CurrentWord.Id,
                    ReviewDate = DateTime.Now,
                    IsCorrect = isCorrect
                };

                await _reviewHistoryRepository.AddAsync(reviewHistory);

                var wordToUpdate = await _wordRepository.GetByIdAsync(CurrentWord.Id);
                if (wordToUpdate != null)
                {
                    wordToUpdate.ReviewCount++;
                    if (isCorrect)
                        wordToUpdate.CorrectCount++;
                    else
                        wordToUpdate.WrongCount++;
                    wordToUpdate.LastReviewed = DateTime.Now;

                    await _wordRepository.UpdateAsync(wordToUpdate);
                }

                // CurrentWord'ü de güncelle (UI için)
                CurrentWord.ReviewCount++;
                if (isCorrect)
                    CurrentWord.CorrectCount++;
                else
                    CurrentWord.WrongCount++;
                CurrentWord.LastReviewed = DateTime.Now;

                // Otomatik olarak bir sonraki karta geç
                if (CanGoNext())
                {
                    await Task.Delay(300);
                    NextCard();
                }
                else
                {
                    // KARTLAR BİTTİ - TEBRİK MESAJI
                    var result = MessageBox.Show(
                        $"🎉 Tebrikler! Tüm kartları tamamladınız.\n\n" +
                        $"✅ Doğru: {CorrectCount}\n" +
                        $"❌ Yanlış: {WrongCount}\n" +
                        $"📊 Başarı Oranı: {SuccessRate:F1}%\n\n" +
                        $"Yeniden başlamak ister misiniz?",
                        "Oturum Tamamlandı",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        RestartSession();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cevap kaydedilirken hata oluştu: {ex.Message}",
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShuffleCards()
        {
            if (!_filteredWords.Any()) return;

            if (IsShuffled)
            {
                // Karışıksa orijinal sıraya döndür
                _filteredWords = _originalFilteredWords.ToList();
                IsShuffled = false;
            }
            else
            {
                // Karıştır
                var random = new Random();
                _filteredWords = _filteredWords.OrderBy(x => random.Next()).ToList();
                IsShuffled = true;
            }

            _currentIndex = 0;
            CurrentWord = _filteredWords[_currentIndex];
            IsFlipped = false;
        }

        private void RestartSession()
        {
            CorrectCount = 0;
            WrongCount = 0;
            _currentIndex = 0;
            IsShuffled = false;

            if (_filteredWords.Any())
            {
                CurrentWord = _filteredWords[_currentIndex];
                IsFlipped = false;
            }
        }

        #endregion
    }
}