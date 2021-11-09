using MetroRadiance.Platform;
using MetroRadiance.UI.Controls;
using SylphyHorn.Interop;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SylphyHorn.UI
{
	partial class TaskTrayTestWindow
	{
		public TaskTrayTestWindow()
		{
			this.InitializeComponent();

			this.ThemeMode = BlurWindowThemeMode.Dark;
			this.LightRadioButton.Click += (sender, e) => this.ThemeMode = BlurWindowThemeMode.Light;
			this.DarkRadioButton.Click += (sender, e) => this.ThemeMode = BlurWindowThemeMode.Dark;
			this.AccentRadioButton.Click += (sender, e) => this.ThemeMode = BlurWindowThemeMode.Accent;

			this.DataContext = new TaskTrayIconsTestViewModel(Theme.Dark, false);
		}

		protected override void OnThemeModeChanged(DependencyPropertyChangedEventArgs e)
		{
			var themeMode = (BlurWindowThemeMode)e.NewValue;
			this.DataContext = new TaskTrayIconsTestViewModel(
				themeMode == BlurWindowThemeMode.Light ? Theme.Light : Theme.Dark,
				themeMode == BlurWindowThemeMode.Accent);

			switch (themeMode)
			{
				case BlurWindowThemeMode.Dark:
					this.LightRadioButton.IsChecked = false;
					this.DarkRadioButton.IsChecked = true;
					this.AccentRadioButton.IsChecked = false;
					break;

				case BlurWindowThemeMode.Accent:
					this.LightRadioButton.IsChecked = false;
					this.DarkRadioButton.IsChecked = false;
					this.AccentRadioButton.IsChecked = true;
					break;

				default:
					this.LightRadioButton.IsChecked = true;
					this.DarkRadioButton.IsChecked = false;
					this.AccentRadioButton.IsChecked = false;
					break;
			}
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

			var position = e.GetPosition(this);
			var result = VisualTreeHelper.HitTest(this.CaptionBar, position);
			if (result != null)
			{
				this.DragMove();
			}
		}

		private class TaskTrayIconTestViewModel
		{
			public TaskTrayIconTestViewModel(double scale, Theme theme, bool colorPrevalence)
			{
				this.Header = $"{100.0 * scale}%";
				this.Width = Math.Round(scale * 16.0);
				this.Height = Math.Round(scale * 16.0);

				var colorName = theme == Theme.Light
					? ImmersiveColorNames.SystemBackgroundLightTheme
					: ImmersiveColorNames.SystemBackgroundDarkTheme;
				this.OpacityMask = new SolidColorBrush(ImmersiveColor.GetColorByTypeName(colorName));

				var dpi = (uint)(96.0 * scale);
				var generator = new DynamicInfoTrayIcon(
					theme,
					colorPrevalence,
					new MetroRadiance.Interop.Dpi(dpi, dpi));
				this.Icon1By3 = generator.GetDesktopInfoIcon(1, 3).ToBitmapSource();
				this.Icon2By7 = generator.GetDesktopInfoIcon(2, 7).ToBitmapSource();
				this.Icon9By9 = generator.GetDesktopInfoIcon(9, 9).ToBitmapSource();
				this.Icon5By10 = generator.GetDesktopInfoIcon(5, 10).ToBitmapSource();
				this.Icon2By100 = generator.GetDesktopInfoIcon(2, 100).ToBitmapSource();
				this.Icon14By100 = generator.GetDesktopInfoIcon(14, 100).ToBitmapSource();
				this.Icon99By100 = generator.GetDesktopInfoIcon(99, 100).ToBitmapSource();
				this.Icon5By1000 = generator.GetDesktopInfoIcon(5, 1000).ToBitmapSource();
				this.Icon52By1000 = generator.GetDesktopInfoIcon(52, 1000).ToBitmapSource();
				this.Icon834By1000 = generator.GetDesktopInfoIcon(834, 1000).ToBitmapSource();
				this.Icon999By1000 = generator.GetDesktopInfoIcon(999, 1000).ToBitmapSource();
				this.Icon1000By1024 = generator.GetDesktopInfoIcon(1000, 1024).ToBitmapSource();
				this.Icon100000By16777216 = generator.GetDesktopInfoIcon(100000, 16777216).ToBitmapSource();
			}

			public string Header { get; }
			public double Width { get; }
			public double Height { get; }
			public SolidColorBrush OpacityMask { get; }

			public ImageSource Icon1By3 { get; }
			public ImageSource Icon2By7 { get; }
			public ImageSource Icon9By9 { get; }
			public ImageSource Icon5By10 { get; }
			public ImageSource Icon2By100 { get; }
			public ImageSource Icon14By100 { get; }
			public ImageSource Icon99By100 { get; }
			public ImageSource Icon5By1000 { get; }
			public ImageSource Icon52By1000 { get; }
			public ImageSource Icon834By1000 { get; }
			public ImageSource Icon999By1000 { get; }
			public ImageSource Icon1000By1024 { get; }
			public ImageSource Icon100000By16777216 { get; }
		}

		private class TaskTrayIconsTestViewModel
		{
			public TaskTrayIconsTestViewModel(Theme theme, bool colorPrevalence)
			{
				this.Scales = new Collection<TaskTrayIconTestViewModel>()
				{
					new TaskTrayIconTestViewModel(1, theme, colorPrevalence),
					new TaskTrayIconTestViewModel(1.25, theme, colorPrevalence),
					new TaskTrayIconTestViewModel(1.5, theme, colorPrevalence),
					new TaskTrayIconTestViewModel(1.75, theme, colorPrevalence),
					new TaskTrayIconTestViewModel(2, theme, colorPrevalence),
					new TaskTrayIconTestViewModel(2.25, theme, colorPrevalence),
					new TaskTrayIconTestViewModel(2.5, theme, colorPrevalence),
					new TaskTrayIconTestViewModel(2.75, theme, colorPrevalence),
					new TaskTrayIconTestViewModel(3, theme, colorPrevalence),
				};
			}

			public double HorizontalFontSize { get; } = 9;

			public double VerticalFontSize { get; } = 8;

			public double Vertical1000FontSize { get; } = 7;

			public FontFamily FontFamily { get; } = new FontFamily("Segoe UI");

			public FontWeight FontWeight { get; } = FontWeights.Bold;

			public Collection<TaskTrayIconTestViewModel> Scales { get; }
		}
	}
}
