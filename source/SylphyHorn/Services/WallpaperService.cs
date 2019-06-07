using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using SylphyHorn.Interop;
using SylphyHorn.Serialization;
using WindowsDesktop;

namespace SylphyHorn.Services
{
	public class WallpaperService : IDisposable
	{
		private static readonly string[] _supportedExtensions = { ".png", ".jpg", ".jpeg", ".bmp", };

		public static WallpaperService Instance { get; } = new WallpaperService();

		private WallpaperService()
		{
			VirtualDesktop.CurrentChanged += this.VirtualDesktopOnCurrentChanged;
		}

		private void VirtualDesktopOnCurrentChanged(object sender, VirtualDesktopChangedEventArgs e)
		{
			Task.Run(() =>
			{
				if (!Settings.General.ChangeBackgroundEachDesktop) return;

				var desktops = VirtualDesktop.GetDesktops();
				var newIndex = Array.IndexOf(desktops, e.NewDesktop) + 1;

				var wallpapers = this.GetWallpaperFiles(Settings.General.DesktopBackgroundFolderPath);
				var files = wallpapers.Where(x => x.DesktopIndex == newIndex).ToArray();
				if (files.Length == 0)
				{
					var file = wallpapers.SingleOrDefault(x => x.Number == 0);
					if (file != null) files = new[] { file };
				}
				if (files.Length != 0) this.Set(files);
			});
		}

		public WallpaperFile[] GetWallpaperFiles(string directoryPath)
		{
			try
			{
				var directoryInfo = new DirectoryInfo(string.IsNullOrEmpty(directoryPath) ? "dummy" : directoryPath);
				if (directoryInfo.Exists)
				{
					var col = new Collection<WallpaperFile>();
					foreach (var file in directoryInfo.GetFiles())
					{
						if (_supportedExtensions.Any(x => x == file.Extension))
						{
							var wallpaper = WallpaperFile.CreateFromFile(file);
							col.Add(wallpaper);
						}
					}

					return col.OrderBy(w => w.Number).ToArray();
				}
			}
			catch (Exception ex)
			{
				LoggingService.Instance.Register(ex);
			}

			return Array.Empty<WallpaperFile>();
		}

		public void Dispose()
		{
			VirtualDesktop.CurrentChanged -= this.VirtualDesktopOnCurrentChanged;
		}

		private void Set(WallpaperFile[] files)
		{
			var dw = DesktopWallpaperFactory.Create();
			var pathes = Enumerable.Range(0, (int)dw.GetMonitorDevicePathCount()).Select(idx => dw.GetMonitorDevicePathAt((uint)idx)).ToArray();
			var merged = files.Length == pathes.Length
				? pathes.Zip(files, (f, p) => Tuple.Create(f, p)).ToList()
				: Enumerable.Repeat(files[0], pathes.Length).Zip(pathes, (p, f) => Tuple.Create(f, p)).ToList();
			merged.ForEach(i => dw.SetWallpaper(i.Item1, i.Item2.Filepath));

			var first = merged.First();
			if (first != null) dw.SetPosition((DesktopWallpaperPosition)first.Item2.Position);
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

	public class WallpaperFile
	{
		/// <summary>
		/// {desktopIndex}-{monitorIndex}-{modeOptions}.{ext}
		/// </summary>
		public string Filepath { get; }

		public ushort DesktopIndex { get; }

		public ushort MonitorIndex { get; }

		public WallpaperPosition Position { get; }

		public uint Number => (uint)(this.DesktopIndex << 16 | this.MonitorIndex);

		public string DesktopMonitorText => this.MonitorIndex == 0 ? this.DesktopIndex.ToString() : $"{this.DesktopIndex}-{this.MonitorIndex}";

		private WallpaperFile(string path, ushort desktopIndex, ushort monitorIndex, WallpaperPosition position)
		{
			this.Filepath = path;
			this.DesktopIndex = desktopIndex;
			this.MonitorIndex = monitorIndex;
			this.Position = position;
		}

		public static WallpaperFile CreateFromFile(FileInfo file)
		{
			var identifiers = Path.GetFileNameWithoutExtension(file.Name).Split('-');

			ushort desktop = 0;
			ushort monitor = 0;
			var position = WallpaperPosition.Fill;

			if (identifiers.Length > 0 && ushort.TryParse(identifiers[0], out desktop))
			{
				if (identifiers.Length > 1 && ushort.TryParse(identifiers[1], out monitor))
				{
					if (identifiers.Length > 2 && identifiers[2].Length >= 1)
					{
						position = Parse(identifiers[2]);
					}
				}
			}

			return new WallpaperFile(file.FullName, desktop, monitor, position);
		}

		private static WallpaperPosition Parse(string options)
		{
			var options2 = options.ToLower();
			if (options2[0] == 'c') return WallpaperPosition.Center;
			if (options2[0] == 't') return WallpaperPosition.Tile;
			if (options2[0] == 's') return WallpaperPosition.Stretch;
			if (options2[0] == 'f') return WallpaperPosition.Fit;
			if (options2.StartsWith("fil")) return WallpaperPosition.Fill;
			if (options2.StartsWith("sp")) return WallpaperPosition.Span;
			return WallpaperPosition.Fill;
		}
	}
}
