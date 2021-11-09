using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MetroRadiance.Interop;
using MetroRadiance.Platform;
using SylphyHorn.Interop;

namespace SylphyHorn.UI
{
	public class DynamicInfoTrayIcon
	{
		private const string _defaultFontFamilyName = "Segoe UI";
		private const double _horizontalFontSize = 9;
		private const double _verticalFontSize = 8;
		private const double _verticalSpacing = -0.5;
		private const double _triggerFontSizeInEffectivePixels = 14.0;
		private const double _minFontSize = 4.0;

		private static readonly SolidColorBrush _lightForegroundBrush = new SolidColorBrush(ImmersiveColor.GetColorByTypeName(ImmersiveColorNames.SystemTextLightTheme));
		private static readonly SolidColorBrush _lightBackgroundBrush = new SolidColorBrush(ImmersiveColor.GetColorByTypeName(ImmersiveColorNames.SystemBackgroundLightTheme));
		private static readonly SolidColorBrush _darkForegroundBrush = new SolidColorBrush(ImmersiveColor.GetColorByTypeName(ImmersiveColorNames.SystemTextDarkTheme));
		private static readonly SolidColorBrush _darkBackgroundBrush = new SolidColorBrush(ImmersiveColor.GetColorByTypeName(ImmersiveColorNames.SystemBackgroundDarkTheme));

		private static readonly Typeface _defaultFont = new Typeface(new FontFamily(_defaultFontFamilyName), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);

		private SolidColorBrush _foregroundBrush;
		private SolidColorBrush _backgroundBrush;
		private Dpi? _dpi;

		public DynamicInfoTrayIcon(Theme theme, bool colorPrevalence, Dpi? dpi = null)
		{
			(this._foregroundBrush, this._backgroundBrush) = GetThemeBrushes(theme, colorPrevalence);
			this._dpi = dpi;
		}

		public System.Drawing.Icon GetDesktopInfoIcon(int currentDesktop, int totalDesktopCount)
		{
			using (var iconBitmap = this.DrawInfo(currentDesktop, totalDesktopCount))
			{
				return iconBitmap.ToIcon();
			}
		}

		public void UpdateBrush(Theme theme, bool colorPrevalence)
		{
			(this._foregroundBrush, this._backgroundBrush) = GetThemeBrushes(theme, colorPrevalence);
		}

		// consolidate two methods below?
		private System.Drawing.Bitmap DrawInfo(int currentDesktop, int totalDesktopCount)
		{
			var iconSize = IconHelper.GetIconSize();
			var dpi = this._dpi ?? GetDpi();
			var scale = dpi.X / 96.0;

			var drawingVisual = new DrawingVisual();
			using (var context = drawingVisual.RenderOpen())
			{
				context.DrawRectangle(this._backgroundBrush, null, new Rect(0.0, 0.0, iconSize.Width, iconSize.Height));

				var currentOrientation = GetOrientation(totalDesktopCount);
				if (currentOrientation == Orientation.Horizontal)
				{
					this.DrawHorizontalInfo(context, iconSize, scale, currentDesktop, totalDesktopCount);
				}
				else
				{
					this.DrawVerticalInfo(context, iconSize, scale, currentDesktop, totalDesktopCount);
				}
			}

			return drawingVisual.ToBitmap(
				iconSize,
				dpi,
				this._backgroundBrush.Color.ToGDIColor());
		}

		private void DrawHorizontalInfo(DrawingContext context, System.Drawing.Size size, double scale, int currentDesktop, int totalDesktopCount)
		{
			var stringToDraw = $"{currentDesktop}/{totalDesktopCount}";
			var formattedText = this.GetFormattedTextFromText(stringToDraw, _horizontalFontSize, size, scale);
			formattedText.LineHeight = Math.Min(_horizontalFontSize, size.Height);

			var offsetY = Math.Floor(0.5 * (size.Height - formattedText.Extent));
			context.DrawText(formattedText, new Point(0, offsetY));
		}

		private void DrawVerticalInfo(DrawingContext context, System.Drawing.Size size, double scale, int currentDesktop, int totalDesktopCount, double? currentFontSize = null)
		{
			var fontSize = currentFontSize ?? _verticalFontSize;
			var scaleable = (fontSize - 1) >= _minFontSize;
			var lineHeight = Math.Min(fontSize, 0.5 * size.Height);

			var firstString = currentDesktop.ToString();
			var firstFormattedText = this.GetFormattedTextFromText(firstString, fontSize, size, scale);
			firstFormattedText.LineHeight = lineHeight;
			if (scaleable && firstFormattedText.MinWidth > size.Width)
			{
				DrawVerticalInfo(context, size, scale, currentDesktop, totalDesktopCount, fontSize - 1);
				return;
			}

			var secondString = totalDesktopCount.ToString();
			var secondFormattedText = this.GetFormattedTextFromText(secondString, fontSize, size, scale);
			secondFormattedText.LineHeight = lineHeight;
			if (scaleable && secondFormattedText.MinWidth > size.Width)
			{
				DrawVerticalInfo(context, size, scale, currentDesktop, totalDesktopCount, fontSize - 1);
				return;
			}

			var offsetY1 = Math.Floor(0.5 * size.Height - _verticalSpacing - firstFormattedText.Extent);
			context.DrawText(firstFormattedText, new Point(0, offsetY1));

			var offsetY2 = Math.Ceiling(0.5 * size.Height + _verticalSpacing);
			context.DrawText(secondFormattedText, new Point(0, offsetY2));
		}

		private FormattedText GetFormattedTextFromText(string text, double fontSize, System.Drawing.Size size, double scale = 1)
		{
			var formattedText = new FormattedText(
				text,
				CultureInfo.CurrentUICulture,
				FlowDirection.LeftToRight,
				_defaultFont,
				fontSize,
				this._foregroundBrush,
				null,
				fontSize * scale >= _triggerFontSizeInEffectivePixels ? TextFormattingMode.Ideal : TextFormattingMode.Display,
				scale);
			formattedText.MaxLineCount = 1;
			formattedText.MaxTextWidth = size.Width;
			formattedText.TextAlignment = TextAlignment.Center;
			formattedText.Trimming = TextTrimming.None;
			return formattedText;
		}

		private static Orientation GetOrientation(int totalDesktopCount)
		{
			return totalDesktopCount >= 10 ? Orientation.Vertical : Orientation.Horizontal;
		}

		private static (SolidColorBrush Foreground, SolidColorBrush Background) GetThemeBrushes(Theme theme, bool colorPrevalence)
		{
			return colorPrevalence
				? (_darkForegroundBrush, new SolidColorBrush(ImmersiveColor.GetColorByTypeName(ImmersiveColorNames.SystemAccentDark1)))
				: theme == Theme.Light
					? (_lightForegroundBrush, _lightBackgroundBrush)
					: (_darkForegroundBrush, _darkBackgroundBrush);
		}

		private static Dpi GetDpi()
		{
			var dpi = IconHelper.GetSystemDpi();
			var maxDpi = Math.Max(dpi.X, dpi.Y);
			var newDpi = new Dpi(maxDpi, maxDpi);
			return newDpi;
		}
	}
}
