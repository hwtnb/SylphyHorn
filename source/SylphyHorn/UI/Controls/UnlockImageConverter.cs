using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using SylphyHorn.Services;

namespace SylphyHorn.UI.Controls
{
	public class UnlockImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return null;

				var filename = (string)value;
				if (string.IsNullOrEmpty(filename)) return null;

				using (var stream = new FileStream(filename, FileMode.Open))
				{
					var decoder = BitmapDecoder.Create(
						stream,
						BitmapCreateOptions.None,
						BitmapCacheOption.OnLoad);

					var bitmap = new WriteableBitmap(decoder.Frames[0]);
					bitmap.Freeze();

					return bitmap;
				}
			}
			catch (Exception ex)
			{
				LoggingService.Instance.Register(ex);
				return null;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
