using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using SylphyHorn.Interop;
using SylphyHorn.Properties;
using SylphyHorn.Serialization;
using WindowsDesktop;

namespace SylphyHorn.Services
{
	public class WallpaperService : IDisposable
	{
		private static readonly WallpaperPosition _defaultPosition = WallpaperPosition.Fill;

		private static readonly ImageFormatSupportDetector[] _detectors =
		{
			new JpegXrSupportDetector(),
			new WebPSupportDetector(),
			new HeifSupportDetector(),
		};

		private static readonly Tuple<string, string, string>[] _defaultSupportedFormats =
		{
			Tuple.Create("JPEG", "JPEG", "*.jpg;*.jpeg;*.jpe;*.jfif"),
			Tuple.Create("PNG", "PNG", "*.png"),
			Tuple.Create("BMP", "Bitmap", "*.bmp;*.dib"),
			Tuple.Create("GIF", "GIF", "*.gif"),
			Tuple.Create("TIFF", "TIFF", "*.tif;*.tiff"),
		};

		public static readonly string SupportedFormats = CreateSupportFormatText();
		public static string[] SupportedFileTypes { get; } = _defaultSupportedFormats
			.Select(f => f.Item1)
			.Concat(_detectors.Where(d => d.IsSupported).Select(d => d.FileType))
			.ToArray();

		public static WallpaperService Instance { get; } = new WallpaperService();

		private WallpaperService()
		{
			if (ProductInfo.IsWallpaperSupportBuild)
			{
				VirtualDesktop.CurrentChanged += this.VirtualDesktopOnCurrentChanged;
			}
			else
			{
				VirtualDesktop.CurrentChanged += this.VirtualDesktopOnCurrentChangedForWin10;
			}
		}

		private void VirtualDesktopOnCurrentChanged(object sender, VirtualDesktopChangedEventArgs e)
		{
			Task.Run(() => SetPosition(e.NewDesktop));
		}

		private void VirtualDesktopOnCurrentChangedForWin10(object sender, VirtualDesktopChangedEventArgs e)
		{
			Task.Run(() =>
			{
				if (!Settings.General.ChangeBackgroundEachDesktop) return;
				SetWallpaperAndPosition(e.NewDesktop);
			});
		}

		public void Dispose()
		{
			VirtualDesktop.CurrentChanged -= this.VirtualDesktopOnCurrentChanged;
		}

		public static void SetPosition(VirtualDesktop newDesktop)
		{
			var newIndex = newDesktop.Index;
			var positionSettings = Settings.General.DesktopBackgroundPositions;
			var positionCount = positionSettings.Count;

			if (positionCount == 0) return;

			var dw = DesktopWallpaperFactory.Create();
			var oldPosition = dw.GetPosition();
			var newPosition = newIndex < positionCount
				? (DesktopWallpaperPosition)positionSettings.Value[newIndex].Value
				: (DesktopWallpaperPosition)_defaultPosition;
			if (oldPosition != newPosition) dw.SetPosition(newPosition);
		}

		public static void SetWallpaperAndPosition(VirtualDesktop newDesktop)
		{
			var newIndex = newDesktop.Index;
			var pathSettings = Settings.General.DesktopBackgroundImagePaths;
			var positionSettings = Settings.General.DesktopBackgroundPositions;
			var pathCount = pathSettings.Count;
			var positionCount = positionSettings.Count;

			if (pathCount == 0 && positionCount == 0) return;

			var dw = DesktopWallpaperFactory.Create();
			var path = newIndex < pathCount
				? pathSettings.Value[newIndex].Value
				: pathSettings.Value.FirstOrDefault(p => p.Value.Length > 0);
			if (!string.IsNullOrEmpty(path)) dw.SetWallpaper(null, path);
			var oldPosition = dw.GetPosition();
			var newPosition = newIndex < positionCount
				? (DesktopWallpaperPosition)positionSettings.Value[newIndex].Value
				: (DesktopWallpaperPosition)_defaultPosition;
			if (oldPosition != newPosition) dw.SetPosition(newPosition);
		}

		public static void SetBackgroundColor(Color color)
		{
			var dw = DesktopWallpaperFactory.Create();
			dw.SetBackgroundColor(new COLORREF { R = color.R, G = color.G, B = color.B });
		}

		public static Tuple<Color, string> GetCurrentColorAndWallpaper()
		{
			var dw = DesktopWallpaperFactory.Create();
			var colorref = dw.GetBackgroundColor();

			string path = null;
			if (dw.GetMonitorDevicePathCount() >= 1)
			{
				var monitorId = dw.GetMonitorDevicePathAt(0);
				path = dw.GetWallpaper(monitorId);
			}

			return Tuple.Create(Color.FromRgb(colorref.R, colorref.G, colorref.B), path);
		}

		private static string CreateSupportFormatText()
		{
			var defaultExtensions = string.Join(
				";",
				_defaultSupportedFormats
					.Select(f => f.Item3)
					.Concat(_detectors.Where(d => d.IsSupported)
						.SelectMany(d => d.Extensions.Select(e => $"*{e}"))));

			return $"Image File ({defaultExtensions})|{defaultExtensions}|" +
				string.Join(
					"|",
					_defaultSupportedFormats
						.Select(f => $"{f.Item2} ({f.Item3})|{f.Item3}")
						.Concat(_detectors.Where(d => d.IsSupported)
							.Select(d => d.FormatInfo)));
		}

		private static WallpaperPosition Parse(string options)
		{
			var options2 = options.ToLower();
			if (options2.StartsWith("fil")) return WallpaperPosition.Fill;
			if (options2.StartsWith("sp")) return WallpaperPosition.Span;
			if (options2[0] == 'c') return WallpaperPosition.Center;
			if (options2[0] == 't') return WallpaperPosition.Tile;
			if (options2[0] == 's') return WallpaperPosition.Stretch;
			if (options2[0] == 'f') return WallpaperPosition.Fit;
			return WallpaperPosition.Fill;
		}
	}

	public enum WallpaperPosition : byte
	{
		Center = 0,
		Tile,
		Stretch,
		Fit,
		Fill,
		Span,
	}
}
