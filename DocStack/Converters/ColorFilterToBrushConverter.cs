using Models.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace DocStack.Converters
{
    public class ColorFilterToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ColorFilters colorFilter)
            {
                return colorFilter switch
                {
                    ColorFilters.Gray => GetResourceBrush("ModernGrayBrush"),
                    ColorFilters.Red => GetResourceBrush("ModernRedBrush"),
                    ColorFilters.Purple => GetResourceBrush("ModernPurpleBrush"),
                    ColorFilters.Blue => GetResourceBrush("ModernBlueBrush"),
                    ColorFilters.Yellow => GetResourceBrush("ModernYellowBrush"),
                    ColorFilters.Green => GetResourceBrush("ModernGreenBrush"),
                    _ => Brushes.Transparent
                };
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); 
        }
        private SolidColorBrush GetResourceBrush(string resourceKey)
        {
            return App.Current.Resources.Contains(resourceKey)
                ? (SolidColorBrush)App.Current.Resources[resourceKey]
                : Brushes.Transparent;
        }
    }
}
