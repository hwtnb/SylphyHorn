using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace SylphyHorn.UI.Converters
{
	public class ArrayToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Join(parameter as string ?? ", ", value as object[]);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
