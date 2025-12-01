using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace VocabularyMaster.WPF.Converters
{
    public class SelectedWordBorderConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] != null && values[1] != null)
            {
                var currentWord = values[0];
                var selectedWord = values[1];

                if (currentWord.Equals(selectedWord))
                {
                    return new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Kırmızı
                }
            }

            return new SolidColorBrush(Color.FromRgb(224, 224, 224)); // Gri
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SelectedWordThicknessConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] != null && values[1] != null)
            {
                var currentWord = values[0];
                var selectedWord = values[1];

                if (currentWord.Equals(selectedWord))
                {
                    return new System.Windows.Thickness(3); // Kalın
                }
            }

            return new System.Windows.Thickness(1); // Normal
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}