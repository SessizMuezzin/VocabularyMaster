using System.Windows;
using System.Windows.Controls;

namespace VocabularyMaster.WPF.Views
{
    public partial class FlashcardView : UserControl
    {
        public FlashcardView()
        {
            InitializeComponent();
        }

        // XAML tarafında "Click='Card_Click'" dendiği için bu metodun burada olması zorunlu.
        private void Card_Click(object sender, RoutedEventArgs e)
        {
            // Kart çevirme animasyonu veya mantığı buradaydıysa tekrar eklenmesi gerekebilir.
            // Şimdilik boş bırakıyoruz ki hata gitsin ve proje çalışsın.
        }
    }
}