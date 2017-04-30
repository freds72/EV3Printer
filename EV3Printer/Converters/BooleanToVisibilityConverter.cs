using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace EV3Printer.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        private bool _isReversed;
        
        /// <summary>
        /// Gets or sets a value indicating whether the return values should be reversed.
        /// </summary>
        public bool IsReversed
        {
            get { return this._isReversed; }
            set { this._isReversed = value; }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);

            if (this.IsReversed)
                val = !val;

            return val ? Visibility.Visible : Visibility.Collapsed;
        }     

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!(value is Visibility))
                return DependencyProperty.UnsetValue;

            var visibility = (Visibility)value;
            var result = visibility == Visibility.Visible;

            return this.IsReversed ? !result : result;
        }
    }
}
