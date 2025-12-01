using System.Windows;
using System.Windows.Controls;
using VocabularyMaster.Core.Models;

namespace VocabularyMaster.WPF.Views
{
    public partial class WordListView : UserControl
    {
        public WordListView()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Word word)
            {
                if (DataContext is ViewModels.WordListViewModel viewModel)
                {
                    viewModel.SelectedWord = word;
                }
            }
        }
    }
}