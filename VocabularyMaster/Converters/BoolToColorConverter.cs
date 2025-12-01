using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace VocabularyMaster.WPF.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked)
            {
                return Color.FromRgb(33, 150, 243); // Mavi
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}