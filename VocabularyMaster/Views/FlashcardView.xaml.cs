using System.Windows.Controls;
using System.Windows.Input;

namespace VocabularyMaster.WPF.Views
{
    public partial class FlashcardView : UserControl
    {
        public FlashcardView()
        {
            InitializeComponent();
        }

        private void Card_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ViewModels.FlashcardViewModel viewModel)
            {
                viewModel.FlipCardCommand.Execute(null);
            }
        }
    }
}