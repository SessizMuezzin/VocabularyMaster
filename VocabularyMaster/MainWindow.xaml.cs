using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VocabularyMaster.WPF.ViewModels;

namespace VocabularyMaster.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // İlk açılışta Dashboard seçili
            SetActiveButton(DashboardButton);
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                SetActiveButton(button);
            }
        }

        private void SetActiveButton(Button activeButton)
        {
            // Tüm butonları normal renge döndür
            DashboardButton.Background = Brushes.Transparent;
            WordsButton.Background = Brushes.Transparent;
            TestButton.Background = Brushes.Transparent;
            StatisticsButton.Background = Brushes.Transparent;
            FlashcardButton.Background = Brushes.Transparent;

            // Aktif butonu vurgula
            activeButton.Background = new SolidColorBrush(Color.FromRgb(52, 73, 94)); // #34495e
        }
    }
}