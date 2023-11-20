using System;
using System.Drawing;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FillingTriangles
{
    class DrawingToMediaColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var clr = (System.Drawing.Color)value;
            return System.Windows.Media.Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var clr = (System.Windows.Media.Color)value;
            return System.Drawing.Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
        }
    }
}
