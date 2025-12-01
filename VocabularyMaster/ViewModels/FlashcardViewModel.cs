using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VocabularyMaster.Core.Interfaces;
using VocabularyMaster.Core.Models;
using VocabularyMaster.WPF.Commands;

namespace VocabularyMaster.WPF.ViewModels
{
    public class FlashcardViewModel : ViewModelBase
    {
        private readonly IWordRepository _wordRepository;
        private readonly IReviewHistoryRepository _reviewHistoryRepository;
        private List<Word> _allWords = new();
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

        public string ProgressText => HasWords ? $"{_currentIndex + 1} / {_allWords.Count}" : "0 / 0";

        public bool HasWords => _allWords.Any();

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

        #endregion

        #region Commands

        public ICommand FlipCardCommand { get; }
        public ICommand NextCardCommand { get; }
        public ICommand PreviousCardCommand { get; }
        public ICommand MarkCorrectCommand { get; }
        public ICommand MarkWrongCommand { get; }
        public ICommand ShuffleCommand { get; }
        public ICommand RestartCommand { get; }

        #endregion

        #region Methods

        private async Task LoadWordsAsync()
        {
            try
            {
                var words = await _wordRepository.GetAllAsync();
                _allWords = words.ToList();

                if (_allWords.Any())
                {
                    _currentIndex = 0;
                    CurrentWord = _allWords[_currentIndex];
                    IsFlipped = false;
                }

                OnPropertyChanged(nameof(HasWords));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kelimeler yüklenirken hata oluştu: {ex.Message}",
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                CurrentWord = _allWords[_currentIndex];
                IsFlipped = false;
            }
        }

        private bool CanGoNext()
        {
            return _allWords.Any() && _currentIndex < _allWords.Count - 1;
        }

        private void PreviousCard()
        {
            if (CanGoPrevious())
            {
                _currentIndex--;
                CurrentWord = _allWords[_currentIndex];
                IsFlipped = false;
            }
        }

        private bool CanGoPrevious()
        {
            return _allWords.Any() && _currentIndex > 0;
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

                CurrentWord.ReviewCount++;
                if (isCorrect)
                    CurrentWord.CorrectCount++;
                else
                    CurrentWord.WrongCount++;
                CurrentWord.LastReviewed = DateTime.Now;

                await _wordRepository.UpdateAsync(CurrentWord);

                // Otomatik olarak bir sonraki karta geç
                if (CanGoNext())
                {
                    await Task.Delay(300); // Kısa bir animasyon gecikmesi
                    NextCard();
                }
                else
                {
                    MessageBox.Show($"Tebrikler! Tüm kartları tamamladınız.\n\n" +
                        $"Doğru: {CorrectCount}\n" +
                        $"Yanlış: {WrongCount}\n" +
                        $"Başarı Oranı: {SuccessRate:F1}%",
                        "Oturum Tamamlandı", MessageBoxButton.OK, MessageBoxImage.Information);
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
            if (!_allWords.Any()) return;

            var random = new Random();
            _allWords = _allWords.OrderBy(x => random.Next()).ToList();
            _currentIndex = 0;
            CurrentWord = _allWords[_currentIndex];
            IsFlipped = false;

            MessageBox.Show("Kartlar karıştırıldı!", "Bilgi",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RestartSession()
        {
            CorrectCount = 0;
            WrongCount = 0;
            _currentIndex = 0;

            if (_allWords.Any())
            {
                CurrentWord = _allWords[_currentIndex];
                IsFlipped = false;
            }

            MessageBox.Show("Oturum yeniden başlatıldı!", "Bilgi",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}