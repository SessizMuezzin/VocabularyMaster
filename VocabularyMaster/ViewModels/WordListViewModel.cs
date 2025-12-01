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
        private ObservableCollection<Word> _words;
        private Word? _selectedWord;
        private string _searchText = string.Empty;
        private string? _selectedCategory;
        private DifficultyLevel? _selectedDifficulty;
        private bool _showOnlyFavorites;

        public bool ShowOnlyFavorites
        {
            get => _showOnlyFavorites;
            set
            {
                if (SetProperty(ref _showOnlyFavorites, value))
                {
                    _ = LoadWordsAsync();
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
                    _ = LoadWordsAsync();
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
                    _ = LoadWordsAsync();
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
                    _ = LoadWordsAsync();
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

            _ = LoadWordsAsync();
            _ = LoadCategoriesAsync();
        }

        private async Task LoadWordsAsync()
        {
            var words = await _wordRepository.GetAllAsync();

            // Favori filtresi
            if (ShowOnlyFavorites)
            {
                words = words.Where(w => w.IsFavorite).ToList();
            }

            // Arama - Geliştirme: Örnek cümlede de ara
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                words = words.Where(w =>
                    w.English.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    w.Turkish.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(w.ExampleSentence) && w.ExampleSentence.Contains(SearchText, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "Tümü")
            {
                words = words.Where(w => w.Category == SelectedCategory).ToList();
            }

            if (SelectedDifficulty.HasValue)
            {
                words = words.Where(w => w.DifficultyLevel == SelectedDifficulty.Value).ToList();
            }

            Words.Clear();
            foreach (var word in words)
            {
                Words.Add(word);
            }
        }

        private async Task ToggleFavoriteAsync()
        {
            if (SelectedWord == null) return;

            SelectedWord.IsFavorite = !SelectedWord.IsFavorite;
            await _wordRepository.UpdateAsync(SelectedWord);

            // UI'ı güncelle
            var index = Words.IndexOf(SelectedWord);
            if (index >= 0)
            {
                Words[index] = SelectedWord;
                OnPropertyChanged(nameof(Words));
            }
            await LoadWordsAsync();
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

            // YENİ EKLE - Varsayılan olarak "Tümü" seç
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