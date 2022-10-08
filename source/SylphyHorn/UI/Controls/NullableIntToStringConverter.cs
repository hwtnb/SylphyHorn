using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using SylphyHorn.Services;

namespace SylphyHorn.UI.Converters
{
	public class NullableIntToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				return ((int)value).ToString();
			}
			catch (Exception ex)
			{
				LoggingService.Instance.Register(ex);
				return null;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				var valueAsString = (string)value;
				if (string.IsNullOrWhiteSpace(valueAsString))
				{
					return null;
				}
				else
				{
					return int.Parse(valueAsString);
				}
			}
			catch (Exception)
			{
				return value;
			}
		}
	}
}
