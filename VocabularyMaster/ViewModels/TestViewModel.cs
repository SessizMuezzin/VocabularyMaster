using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VocabularyMaster.Core.Interfaces;
using VocabularyMaster.Core.Models;
using VocabularyMaster.Core.Enums;
using VocabularyMaster.WPF.Commands;
using System.Collections.ObjectModel;

namespace VocabularyMaster.WPF.ViewModels
{
    public class TestViewModel : ViewModelBase
    {
        private readonly IWordRepository _wordRepository;
        private readonly IReviewHistoryRepository _reviewHistoryRepository;
        private DifficultyLevel? _selectedDifficultyLevel;
        private Word? _currentWord;
        private string _userAnswer = string.Empty;
        private string _feedback = string.Empty;
        private bool _isAnswerVisible;
        private TestMode _selectedTestMode = TestMode.EnglishToTurkish;
        private int _correctCount;
        private int _wrongCount;
        private int _currentStreak;
        private int _bestStreak;
        private bool _showOnlyFavorites;
        private ObservableCollection<string> _multipleChoiceOptions;
        private string? _selectedOption;

        public DifficultyLevel? SelectedDifficultyLevel
        {
            get => _selectedDifficultyLevel;
            set
            {
                if (SetProperty(ref _selectedDifficultyLevel, value))
                {
                    _ = LoadNewWordAsync();
                }
            }
        }

        public Word? CurrentWord
        {
            get => _currentWord;
            set => SetProperty(ref _currentWord, value);
        }

        public string UserAnswer
        {
            get => _userAnswer;
            set => SetProperty(ref _userAnswer, value);
        }

        public string Feedback
        {
            get => _feedback;
            set => SetProperty(ref _feedback, value);
        }

        public bool IsAnswerVisible
        {
            get => _isAnswerVisible;
            set => SetProperty(ref _isAnswerVisible, value);
        }

        public TestMode SelectedTestMode
        {
            get => _selectedTestMode;
            set
            {
                if (SetProperty(ref _selectedTestMode, value))
                {
                    OnPropertyChanged(nameof(IsMultipleChoiceMode));
                    OnPropertyChanged(nameof(IsTextInputMode));
                    _ = LoadNewWordAsync();
                }
            }
        }

        public int CorrectCount
        {
            get => _correctCount;
            set => SetProperty(ref _correctCount, value);
        }

        public int WrongCount
        {
            get => _wrongCount;
            set => SetProperty(ref _wrongCount, value);
        }

        public int CurrentStreak
        {
            get => _currentStreak;
            set
            {
                if (SetProperty(ref _currentStreak, value) && value > BestStreak)
                {
                    BestStreak = value;
                }
            }
        }

        public int BestStreak
        {
            get => _bestStreak;
            set => SetProperty(ref _bestStreak, value);
        }

        public string QuestionPrompt => SelectedTestMode == TestMode.EnglishToTurkish
            ? "Türkçe karşılığını yazın:"
            : "İngilizce karşılığını yazın:";

        public string QuestionText
        {
            get
            {
                if (CurrentWord == null) return "";

                if (SelectedTestMode == TestMode.EnglishToTurkish)
                {
                    return CurrentWord.English;
                }
                else
                {
                    // Türkçe → İngilizce testinde rastgele bir anlam göster
                    if (CurrentWord.Meanings.Any())
                    {
                        var random = new Random();
                        var randomMeaning = CurrentWord.Meanings[random.Next(CurrentWord.Meanings.Count)];
                        return randomMeaning.Turkish;
                    }
                    return CurrentWord.Turkish;
                }
            }
        }

        public ObservableCollection<string> MultipleChoiceOptions
        {
            get => _multipleChoiceOptions;
            set => SetProperty(ref _multipleChoiceOptions, value);
        }

        public string? SelectedOption
        {
            get => _selectedOption;
            set => SetProperty(ref _selectedOption, value);
        }

        public bool IsMultipleChoiceMode => SelectedTestMode == TestMode.MultipleChoice;
        public bool IsTextInputMode => SelectedTestMode != TestMode.MultipleChoice;

        public ICommand CheckAnswerCommand { get; }
        public ICommand ShowAnswerCommand { get; }
        public ICommand NextWordCommand { get; }
        public ICommand ResetStatsCommand { get; }

        public TestViewModel(IWordRepository wordRepository, IReviewHistoryRepository reviewHistoryRepository)
        {
            _wordRepository = wordRepository;
            _reviewHistoryRepository = reviewHistoryRepository;
            _multipleChoiceOptions = new ObservableCollection<string>();

            CheckAnswerCommand = new RelayCommand(async _ => await CheckAnswerAsync(), _ => CanCheckAnswer());
            ShowAnswerCommand = new RelayCommand(_ => ShowAnswer(), _ => CurrentWord != null);
            NextWordCommand = new RelayCommand(async _ => await LoadNewWordAsync());
            ResetStatsCommand = new RelayCommand(_ => ResetStats());

            _ = LoadNewWordAsync();
        }

        private bool CanCheckAnswer()
        {
            if (CurrentWord == null) return false;

            if (SelectedTestMode == TestMode.MultipleChoice)
                return !string.IsNullOrEmpty(SelectedOption);
            else
                return !string.IsNullOrWhiteSpace(UserAnswer);
        }

        private async Task LoadNewWordAsync()
        {
            Word? word;

            if (ShowOnlyFavorites)
            {
                if (SelectedDifficultyLevel.HasValue)
                {
                    var allFavorites = await _wordRepository.GetFavoriteWordsAsync();
                    var favoritesInLevel = allFavorites.Where(w => w.DifficultyLevel == SelectedDifficultyLevel.Value).ToList();

                    if (!favoritesInLevel.Any())
                    {
                        MessageBox.Show($"{SelectedDifficultyLevel.Value} seviyesinde favori kelime bulunamadı!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    var random = new Random();
                    word = favoritesInLevel[random.Next(favoritesInLevel.Count)];
                }
                else
                {
                    word = await _wordRepository.GetRandomFavoriteWordAsync();
                }
            }
            else if (SelectedDifficultyLevel.HasValue)
            {
                var wordsInLevel = await _wordRepository.GetByDifficultyAsync(SelectedDifficultyLevel.Value);
                if (!wordsInLevel.Any())
                {
                    MessageBox.Show($"{SelectedDifficultyLevel.Value} seviyesinde kelime bulunamadı!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var random = new Random();
                word = wordsInLevel[random.Next(wordsInLevel.Count)];
            }
            else
            {
                word = await _wordRepository.GetRandomWordAsync();
            }

            if (word == null)
            {
                MessageBox.Show("Henüz kelime eklenmemiş! Lütfen önce kelime ekleyin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CurrentWord = word;
            UserAnswer = string.Empty;
            SelectedOption = null;
            Feedback = string.Empty;
            IsAnswerVisible = false;

            // Çoktan seçmeli için şıkları oluştur
            if (SelectedTestMode == TestMode.MultipleChoice)
            {
                await GenerateMultipleChoiceOptionsAsync();
            }

            OnPropertyChanged(nameof(QuestionPrompt));
            OnPropertyChanged(nameof(QuestionText));
            OnPropertyChanged(nameof(MultipleChoiceOptions));
        }

        public bool ShowOnlyFavorites
        {
            get => _showOnlyFavorites;
            set
            {
                if (SetProperty(ref _showOnlyFavorites, value))
                {
                    _ = LoadNewWordAsync();
                }
            }
        }

        private async Task CheckAnswerAsync()
        {
            if (CurrentWord == null) return;

            string correctAnswer;
            bool isCorrect;

            if (SelectedTestMode == TestMode.MultipleChoice)
            {
                // Çoktan seçmeli
                if (string.IsNullOrEmpty(SelectedOption))
                    return;

                if (SelectedTestMode == TestMode.MultipleChoice && QuestionPrompt.Contains("Türkçe"))
                {
                    var allMeanings = CurrentWord.Meanings.Select(m => m.Turkish).ToList();
                    if (!string.IsNullOrEmpty(CurrentWord.Turkish) && !allMeanings.Contains(CurrentWord.Turkish))
                    {
                        allMeanings.Add(CurrentWord.Turkish);
                    }
                    correctAnswer = string.Join(", ", allMeanings);
                    isCorrect = allMeanings.Any(m => m.Equals(SelectedOption, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    correctAnswer = CurrentWord.English;
                    isCorrect = SelectedOption.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase);
                }
            }
            else
            {
                // Yazılı test
                if (string.IsNullOrWhiteSpace(UserAnswer))
                    return;

                if (SelectedTestMode == TestMode.EnglishToTurkish || QuestionPrompt.Contains("Türkçe"))
                {
                    var allMeanings = CurrentWord.Meanings.Select(m => m.Turkish).ToList();
                    if (!string.IsNullOrEmpty(CurrentWord.Turkish) && !allMeanings.Contains(CurrentWord.Turkish))
                    {
                        allMeanings.Add(CurrentWord.Turkish);
                    }
                    correctAnswer = string.Join(", ", allMeanings);
                    isCorrect = allMeanings.Any(meaning =>
                        UserAnswer.Trim().Equals(meaning, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    correctAnswer = CurrentWord.English;
                    isCorrect = UserAnswer.Trim().Equals(correctAnswer, StringComparison.OrdinalIgnoreCase);
                }
            }

            UpdateWordStats(isCorrect, correctAnswer);
        }

        private void UpdateWordStats(bool isCorrect, string correctAnswer)
        {
            CurrentWord.ReviewCount++;
            CurrentWord.LastReviewed = DateTime.Now;

            if (isCorrect)
            {
                CurrentWord.CorrectCount++;
                CorrectCount++;
                CurrentStreak++;
                Feedback = "✓ Doğru!";
            }
            else
            {
                CurrentWord.WrongCount++;
                WrongCount++;
                CurrentStreak = 0;
                Feedback = $"✗ Yanlış! Doğru cevap: {correctAnswer}";
            }

            _ = _wordRepository.UpdateAsync(CurrentWord);

            var review = new ReviewHistory
            {
                WordId = CurrentWord.Id,
                IsCorrect = isCorrect,
                TestMode = SelectedTestMode,
                ReviewDate = DateTime.Now
            };
            _ = _reviewHistoryRepository.AddAsync(review);

            IsAnswerVisible = true;
        }

        private void ShowAnswer()
        {
            if (CurrentWord == null) return;

            string correctAnswer;

            if (SelectedTestMode == TestMode.EnglishToTurkish)
            {
                var allMeanings = CurrentWord.Meanings.Select(m => m.Turkish).ToList();
                if (!string.IsNullOrEmpty(CurrentWord.Turkish) && !allMeanings.Contains(CurrentWord.Turkish))
                {
                    allMeanings.Add(CurrentWord.Turkish);
                }
                correctAnswer = string.Join(", ", allMeanings);
            }
            else
            {
                correctAnswer = CurrentWord.English;
            }

            Feedback = $"Cevap: {correctAnswer}";
            IsAnswerVisible = true;

            CurrentWord.ReviewCount++;
            CurrentWord.WrongCount++;
            CurrentWord.LastReviewed = DateTime.Now;
            WrongCount++;
            CurrentStreak = 0;

            _ = _wordRepository.UpdateAsync(CurrentWord);

            var review = new ReviewHistory
            {
                WordId = CurrentWord.Id,
                IsCorrect = false,
                TestMode = SelectedTestMode,
                ReviewDate = DateTime.Now
            };
            _ = _reviewHistoryRepository.AddAsync(review);
        }
        private void ResetStats()
        {
            CorrectCount = 0;
            WrongCount = 0;
            CurrentStreak = 0;
            BestStreak = 0;
        }

        private async Task GenerateMultipleChoiceOptionsAsync()
        {
            if (CurrentWord == null) return;

            MultipleChoiceOptions.Clear();
            var random = new Random();
            var correctAnswer = string.Empty;

            if (SelectedTestMode == TestMode.MultipleChoice)
            {
                if (QuestionPrompt.Contains("Türkçe"))
                {
                    // İngilizce → Türkçe
                    var allMeanings = CurrentWord.Meanings.Select(m => m.Turkish).ToList();
                    if (!string.IsNullOrEmpty(CurrentWord.Turkish) && !allMeanings.Contains(CurrentWord.Turkish))
                    {
                        allMeanings.Add(CurrentWord.Turkish);
                    }
                    correctAnswer = allMeanings.FirstOrDefault() ?? CurrentWord.Turkish;
                }
                else
                {
                    // Türkçe → İngilizce
                    correctAnswer = CurrentWord.English;
                }

                // Yanlış şıkları al
                var allWords = await _wordRepository.GetAllAsync();
                var wrongOptions = allWords
                    .Where(w => w.Id != CurrentWord.Id)
                    .OrderBy(x => random.Next())
                    .Take(3)
                    .ToList();

                var options = new List<string> { correctAnswer };

                if (QuestionPrompt.Contains("Türkçe"))
                {
                    // İngilizce → Türkçe için yanlış Türkçe anlamlar
                    foreach (var word in wrongOptions)
                    {
                        if (word.Meanings.Any())
                        {
                            options.Add(word.Meanings.First().Turkish);
                        }
                        else if (!string.IsNullOrEmpty(word.Turkish))
                        {
                            options.Add(word.Turkish);
                        }
                    }
                }
                else
                {
                    // Türkçe → İngilizce için yanlış İngilizce kelimeler
                    options.AddRange(wrongOptions.Select(w => w.English));
                }

                // Karıştır
                var shuffled = options.OrderBy(x => random.Next()).Take(4).ToList();

                foreach (var option in shuffled)
                {
                    MultipleChoiceOptions.Add(option);
                }
            }
        }
    }
}