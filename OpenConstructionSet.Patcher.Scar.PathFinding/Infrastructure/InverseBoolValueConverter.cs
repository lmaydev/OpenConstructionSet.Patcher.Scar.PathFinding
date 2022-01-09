using System;
using System.Globalization;
using System.Windows.Data;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure
{
    internal class InverseBoolValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool) || value is not bool boolValue)
            {
                throw new Exception("value and targetType must be bool");
            }

            return !boolValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool) || value is not bool boolValue)
            {
                throw new Exception("value and targetType must be bool");
            }

            return !boolValue;
        }
    }
}
