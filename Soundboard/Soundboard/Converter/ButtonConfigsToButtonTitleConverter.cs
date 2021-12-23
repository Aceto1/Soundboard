using Soundboard.Model;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace Soundboard.Converter
{
    public class ButtonConfigsToButtonTitleConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ObservableCollection<ButtonConfig> buttonConfigs)
                return "";

            var index = (int)parameter;

            var config = buttonConfigs.SingleOrDefault(m => m.Index == index);

            if (config == null)
                return "Click to edit!";

            else return config.Title;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
