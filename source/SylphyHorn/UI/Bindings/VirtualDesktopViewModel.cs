using Livet;
using SylphyHorn.Properties;
using SylphyHorn.Serialization;
using SylphyHorn.Services;
using System;
using System.IO;
using System.Linq;
using System.Windows.Media;
using WindowsDesktop;

namespace SylphyHorn.UI.Bindings
{
	public class VirtualDesktopViewModel : ViewModel
	{
		private DesktopNameProperty _name;
		private WallpaperViewModel _wallpaper;
		private Action<string> _nameFunc;

		public int Index => this._name.Index;

		public string NumberText => this._name.NumberText;

		#region Name notification property

		public string Name
		{
			get => this._name.Value;
			set => this._nameFunc(value);
		}

		#endregion

		public bool IsWallpaperEnabled => ProductInfo.IsWindows11OrLater || Settings.General.ChangeBackgroundEachDesktop;

		#region WallpaperPath notification property

		public string WallpaperPath
		{
			get => this._wallpaper.FilePath;
			set
			{
				if (this._wallpaper.FilePath != value)
				{
					this._wallpaper.FilePath = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region WallpaperPosition notification property

		public WallpaperPosition WallpaperPosition
		{
			get => this._wallpaper.Position;
			set
			{
				if (this._wallpaper.Position != value)
				{
					this._wallpaper.Position = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region Wallpaper notification property

		public WallpaperViewModel Wallpaper
		{
			get => this._wallpaper;
			set
			{
				if (this._wallpaper != value)
				{
					this._wallpaper = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		public bool HasWallpaper => File.Exists(this.WallpaperPath);

		private VirtualDesktopViewModel(int index, VirtualDesktop desktop)
		{
			var settings = Settings.General;
			var name = settings.DesktopNames.Value[index];
			this._name = name;

			var wallpaperPath = settings.DesktopBackgroundImagePaths.Value[index];
			var wallpaperPosition = settings.DesktopBackgroundPositions.Value[index];
			this._wallpaper = new WallpaperViewModel(desktop, wallpaperPath, wallpaperPosition);

			if (!ProductInfo.IsWindows11OrLater)
			{
				this._nameFunc = n =>
				{
					if (this._name.Value != n)
					{
						this._name.Value = n;
						this.RaisePropertyChanged();
					}
				};
				return;
			}

			if (name.Value != desktop.Name)
			{
				name.Value = desktop.Name;
			}
			this._nameFunc = n =>
			{
				if (this._name.Value != n)
				{
					this._name.Value = n;
					desktop.Name = n;
					this.RaisePropertyChanged();
				}
			};
		}

		public static VirtualDesktopViewModel[] CreateAll()
		{
			var desktops = VirtualDesktop.GetDesktops();
			var desktopCount = desktops.Length;
			var settings = Settings.General;
			if (settings.DesktopNames.Count != desktopCount)
			{
				settings.DesktopNames.Resize(desktopCount);
			}
			if (settings.DesktopBackgroundImagePaths.Count != desktopCount)
			{
				settings.DesktopBackgroundImagePaths.Resize(desktopCount);
			}
			if (settings.DesktopBackgroundPositions.Count != desktopCount)
			{
				settings.DesktopBackgroundPositions.Resize(desktopCount);
			}
			return desktops.Select((d, i) => new VirtualDesktopViewModel(i, d)).ToArray();
		}

		public static VirtualDesktopViewModel Create(VirtualDesktop desktop)
		{
			var desktopIndex = desktop.Index;
			var requiredCount = desktopIndex + 1;
			var settings = Settings.General;
			if (settings.DesktopNames.Count < requiredCount)
			{
				settings.DesktopNames.Resize(requiredCount);
			}
			if (settings.DesktopBackgroundImagePaths.Count < requiredCount)
			{
				settings.DesktopBackgroundImagePaths.Resize(requiredCount);
			}
			if (settings.DesktopBackgroundPositions.Count < requiredCount)
			{
				settings.DesktopBackgroundPositions.Resize(requiredCount);
			}
			return new VirtualDesktopViewModel(desktopIndex, desktop);
		}
	}

	public class WallpaperViewModel : ViewModel
	{
		private WallpaperPathProperty _path;
		private WallpaperPositionsProperty _position;
		private Action<string> _pathFunc;
		private Action<byte> _positionFunc;

		public int DesktopIndex => this._path.Index;

		#region FilePath notification property

		public string FilePath
		{
			get => this._path.Value;
			set => this._pathFunc(value);
		}

		#endregion

		#region Position notification property

		public WallpaperPosition Position
		{
			get => (WallpaperPosition)this._position.Value;
			set => this._positionFunc((byte)value);
		}

		#endregion

		public Color Color
		{
			get => WallpaperService.GetCurrentColorAndWallpaper().Item1;
			set => WallpaperService.SetBackgroundColor(value);
		}

		public WallpaperViewModel(VirtualDesktop desktop, WallpaperPathProperty path, WallpaperPositionsProperty position)
		{
			this._path = path;
			this._position = position;

			this._positionFunc = p =>
			{
				if (this._position.Value != p)
				{
					this._position.Value = p;
					var currentDesktop = VirtualDesktop.Current;
					if (desktop == currentDesktop)
					{
						WallpaperService.SetPosition(currentDesktop);
					}
					this.RaisePropertyChanged();
				}
			};

			if (!ProductInfo.IsWindows11OrLater)
			{
				var generalSettings = Settings.General;
				this._pathFunc = p =>
				{
					if (p == null || p.Length == 0) return;
					if (this._path.Value != p)
					{
						this._path.Value = p;
						if (generalSettings.ChangeBackgroundEachDesktop && desktop == VirtualDesktop.Current)
						{
							WallpaperService.SetWallpaperAndPosition(desktop);
						}
						this.RaisePropertyChanged();
					}
				};
				return;
			}

			if (path.Value != desktop.WallpaperPath)
			{
				path.Value = desktop.WallpaperPath;
			}
			this._pathFunc = p =>
			{
				if (p == null || p.Length == 0) return;
				if (this._path.Value != p)
				{
					this._path.Value = p;
					desktop.WallpaperPath = p;
					this.RaisePropertyChanged();
				}
			};
		}
	}
}
