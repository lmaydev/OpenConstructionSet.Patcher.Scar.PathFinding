using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure
{
    internal class InverseBoolValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool) || !(value is bool))
            {
                throw new Exception("value and targetType must be bool");
            }

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool) || !(value is bool))
            {
                throw new Exception("value and targetType must be bool");
            }

            return !(bool)value;
        }
    }
}
