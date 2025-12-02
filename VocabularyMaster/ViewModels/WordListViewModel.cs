using System;
using System.Collections.ObjectModel;
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
    public class WordListViewModel : ViewModelBase
    {
        private readonly IWordRepository _wordRepository;
        private ObservableCollection<Word> _allWords = new();  // CACHE
        private ObservableCollection<Word> _words;
        private Word? _selectedWord;
        private string _searchText = string.Empty;
        private string? _selectedCategory;
        private DifficultyLevel? _selectedDifficulty;
        private bool _showOnlyFavorites;
        private bool _isInitialized = false;
        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool ShowOnlyFavorites
        {
            get => _showOnlyFavorites;
            set
            {
                if (SetProperty(ref _showOnlyFavorites, value))
                {
                    ApplyFilters();
                }
            }
        }

        public ObservableCollection<Word> Words
        {
            get => _words;
            set => SetProperty(ref _words, value);
        }

        public Word? SelectedWord
        {
            get => _selectedWord;
            set => SetProperty(ref _selectedWord, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilters();
                }
            }
        }

        public string? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    ApplyFilters();
                }
            }
        }

        public DifficultyLevel? SelectedDifficulty
        {
            get => _selectedDifficulty;
            set
            {
                if (SetProperty(ref _selectedDifficulty, value))
                {
                    ApplyFilters();
                }
            }
        }

        public ObservableCollection<string> Categories { get; set; }

        public ICommand AddWordCommand { get; }
        public ICommand EditWordCommand { get; }
        public ICommand DeleteWordCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }

        public WordListViewModel(IWordRepository wordRepository)
        {
            _wordRepository = wordRepository;
            _words = new ObservableCollection<Word>();
            Categories = new ObservableCollection<string>();

            AddWordCommand = new RelayCommand(async _ => await AddWordAsync());
            EditWordCommand = new RelayCommand(async _ => await EditWordAsync(), _ => SelectedWord != null);
            DeleteWordCommand = new RelayCommand(async _ => await DeleteWordAsync(), _ => SelectedWord != null);
            RefreshCommand = new RelayCommand(async _ => await LoadWordsAsync());
            ToggleFavoriteCommand = new RelayCommand(async _ => await ToggleFavoriteAsync(), _ => SelectedWord != null);
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            _isInitialized = true;

            await LoadCategoriesAsync();

            _ = Task.Run(async () =>
            {
                await LoadWordsAsync();
            });
        }

        private async Task LoadWordsAsync()
        {
            IsLoading = true;

            try
            {
                var words = await _wordRepository.GetAllAsync();

                _allWords.Clear();
                foreach (var word in words)
                {
                    _allWords.Add(word);
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kelimeler yüklenirken hata: {ex.Message}", "Hata");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyFilters()
        {
            if (!_isInitialized || !_allWords.Any()) return;

            var filtered = _allWords.AsEnumerable();

            // Kategori filtresi
            if (!string.IsNullOrWhiteSpace(SelectedCategory) && SelectedCategory != "Tümü")
            {
                filtered = filtered.Where(w => w.Category == SelectedCategory);
            }

            // Seviye filtresi
            if (SelectedDifficulty.HasValue)
            {
                filtered = filtered.Where(w => w.DifficultyLevel == SelectedDifficulty.Value);
            }

            // Favori filtresi
            if (ShowOnlyFavorites)
            {
                filtered = filtered.Where(w => w.IsFavorite);
            }

            // Arama filtresi
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(w =>
                    w.English.ToLower().Contains(searchLower) ||
                    w.Turkish.ToLower().Contains(searchLower) ||
                    (w.ExampleSentence != null && w.ExampleSentence.ToLower().Contains(searchLower)) ||
                    w.Meanings.Any(m => m.Turkish.ToLower().Contains(searchLower) ||
                                   (m.ExampleSentence != null && m.ExampleSentence.ToLower().Contains(searchLower)))
                );
            }

            Words = new ObservableCollection<Word>(filtered);
        }

        private async Task ToggleFavoriteAsync()
        {
            if (SelectedWord == null) return;

            SelectedWord.IsFavorite = !SelectedWord.IsFavorite;
            await _wordRepository.UpdateAsync(SelectedWord);
            ApplyFilters();
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _wordRepository.GetAllCategoriesAsync();
            Categories.Clear();
            Categories.Add("Tümü");
            foreach (var category in categories)
            {
                Categories.Add(category);
            }

            if (string.IsNullOrEmpty(SelectedCategory))
            {
                SelectedCategory = "Tümü";
            }
        }

        private async Task AddWordAsync()
        {
            var dialog = new Views.AddEditWordDialog();
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true && dialog.WordData != null)
            {
                await _wordRepository.AddAsync(dialog.WordData);
                await LoadWordsAsync();
                await LoadCategoriesAsync();
                MessageBox.Show("Kelime başarıyla eklendi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task EditWordAsync()
        {
            if (SelectedWord == null) return;

            var dialog = new Views.AddEditWordDialog(SelectedWord);
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true && dialog.WordData != null)
            {
                await _wordRepository.UpdateAsync(dialog.WordData);
                await LoadWordsAsync();
                await LoadCategoriesAsync();
                MessageBox.Show("Kelime başarıyla güncellendi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task DeleteWordAsync()
        {
            if (SelectedWord == null) return;

            var result = MessageBox.Show(
                $"'{SelectedWord.English}' kelimesini silmek istediğinize emin misiniz?",
                "Silme Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _wordRepository.DeleteAsync(SelectedWord.Id);
                await LoadWordsAsync();
                MessageBox.Show("Kelime başarıyla silindi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}