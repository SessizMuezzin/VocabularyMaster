using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VocabularyMaster.Core.Enums;

namespace VocabularyMaster.WPF.Views
{
    public partial class TestView : UserControl
    {
        public TestView()
        {
            InitializeComponent();
        }

        private void DifficultyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ViewModels.TestViewModel viewModel)
            {
                var selectedIndex = DifficultyFilterComboBox.SelectedIndex;

                if (selectedIndex == 0)
                {
                    viewModel.SelectedDifficultyLevel = null;
                }
                else
                {
                    viewModel.SelectedDifficultyLevel = (DifficultyLevel)selectedIndex;
                }
            }
        }

        private void Option_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string option)
            {
                if (DataContext is ViewModels.TestViewModel viewModel)
                {
                    viewModel.SelectedOption = option;
                    UpdateOptionStyles();
                }
            }
        }

        private void UpdateOptionStyles()
        {
            if (DataContext is not ViewModels.TestViewModel viewModel)
                return;

            if (OptionsItemsControl?.Items == null)
                return;

            for (int i = 0; i < OptionsItemsControl.Items.Count; i++)
            {
                var container = OptionsItemsControl.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                if (container == null) continue;

                var border = FindVisualChild<Border>(container);

                if (border != null)
                {
                    var option = OptionsItemsControl.Items[i] as string;
                    bool isSelected = option == viewModel.SelectedOption;

                    if (isSelected)
                    {
                        border.Background = new SolidColorBrush(Color.FromRgb(187, 222, 251)); // #BBDEFB
                        border.BorderBrush = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // #2196F3
                        border.BorderThickness = new Thickness(3);
                    }
                    else
                    {
                        border.Background = Brushes.White;
                        border.BorderBrush = new SolidColorBrush(Color.FromRgb(221, 221, 221)); // #ddd
                        border.BorderThickness = new Thickness(2);
                    }
                }
            }
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                    return typedChild;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
    }
}