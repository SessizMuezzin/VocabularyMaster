using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VocabularyMaster.Core.Models;

namespace VocabularyMaster.WPF.Views
{
    public partial class WordListView : UserControl
    {
        private bool _hasLoaded = false;

        public WordListView()
        {
            InitializeComponent();
            Loaded += WordListView_Loaded;
        }

        private async void WordListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_hasLoaded) return;
            _hasLoaded = true;

            if (DataContext is ViewModels.WordListViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
        }

        private void Favorite_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Tag is Word word)
            {
                if (DataContext is ViewModels.WordListViewModel viewModel)
                {
                    viewModel.SelectedWord = word;
                    viewModel.ToggleFavoriteCommand.Execute(null);
                }
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Word word)
            {
                if (DataContext is ViewModels.WordListViewModel viewModel)
                {
                    viewModel.SelectedWord = word;
                    viewModel.EditWordCommand.Execute(null);
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Word word)
            {
                if (DataContext is ViewModels.WordListViewModel viewModel)
                {
                    viewModel.SelectedWord = word;
                    viewModel.DeleteWordCommand.Execute(null);
                }
            }
        }

        private void DifficultyFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem item)
            {
                if (DataContext is ViewModels.WordListViewModel viewModel)
                {
                    if (item.Tag == null)
                    {
                        viewModel.SelectedDifficulty = null;
                    }
                    else if (item.Tag is VocabularyMaster.Core.Enums.DifficultyLevel level)
                    {
                        viewModel.SelectedDifficulty = level;
                    }
                }
            }
        }
    }
}