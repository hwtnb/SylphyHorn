using System;
using System.Linq;
using SylphyHorn.Properties;
using SylphyHorn.Serialization;
using WindowsDesktop;

namespace SylphyHorn.Services
{
	internal static class SettingsService
	{
		#region Synchronize

		public static void Synchronize(bool overrideDesktops)
		{
			if (ProductInfo.IsNameSupportBuild)
			{
				var generalSettings = Settings.General;
				overrideDesktops = overrideDesktops && (generalSettings.DesktopNames.Count > 0 || generalSettings.DesktopBackgroundImagePaths.Count > 0);
				if (overrideDesktops)
				{
					FitWindowsDesktopsWithList();
					UpdateWindowsDesktopsByList();
					ResizeShortcutList();
				}
				else
				{
					ResizeList();
					SynchronizeWithWindows();
				}
			}
			else
			{
				ResizeList();
			}

			WallpaperService.SetPosition(VirtualDesktop.Current);
		}

		public static void SynchronizeOnStartup()
		{
			Synchronize(Settings.General.OverrideDesktopsOnStartup);
		}

		public static void SynchronizeWithWindows()
		{
			// for OS Build 18975 or later
			var desktops = VirtualDesktop.GetDesktops();

			_synchronizeWithWindowsAction(desktops);
		}

		#endregion

		#region Resize

		public static void ResizeList()
		{
			var desktopCount = VirtualDesktopService.Count;

			ResizeDesktopListCore(desktopCount);
			ResizeShortcutListCore(desktopCount);
		}

		public static void ResizeShortcutList()
		{
			var desktopCount = VirtualDesktopService.Count;

			ResizeShortcutListCore(desktopCount);
		}

		#endregion

		#region private methods

		private static void ResizeDesktopListCore(int desktopCount)
		{
			Settings.General.DesktopNames.Resize(desktopCount);
			Settings.General.DesktopBackgroundImagePaths.Resize(desktopCount);
			Settings.General.DesktopBackgroundPositions.Resize(desktopCount);
		}

		private static void ResizeShortcutListCore(int desktopCount)
		{
			Settings.ShortcutKey.SwitchToIndices.Resize(desktopCount);
			Settings.ShortcutKey.MoveToIndices.Resize(desktopCount);
			Settings.ShortcutKey.SwapDesktopIndices.Resize(desktopCount);

			Settings.MouseShortcut.SwitchToIndices.Resize(desktopCount);
			Settings.MouseShortcut.MoveToIndices.Resize(desktopCount);
			Settings.MouseShortcut.SwapDesktopIndices.Resize(desktopCount);
		}

		private static Action<VirtualDesktop[]> _synchronizeWithWindowsAction = new Func<Action<VirtualDesktop[]>>(() =>
		{
			if (ProductInfo.IsWallpaperSupportBuild)
			{
				return desktops =>
				{
					SynchronizeDesktopNamesCore(desktops);
					SynchronizeWallpaperPathsCore(desktops);
				};
			}
			else if (ProductInfo.IsNameSupportBuild)
			{
				return SynchronizeDesktopNamesCore;
			}
			else
			{
				return _ => { };
			}
		})();

		private static void SynchronizeDesktopNamesCore(VirtualDesktop[] desktops)
		{
			// for OS Build 18975 or later
			var generalSettings = Settings.General;

			Array.ForEach(generalSettings.DesktopNames.Value.ToArray(),
				prop => prop.Value = desktops[prop.Index].Name);
		}

		private static void SynchronizeWallpaperPathsCore(VirtualDesktop[] desktops)
		{
			// for OS Build 21337 or later
			var generalSettings = Settings.General;

			Array.ForEach(generalSettings.DesktopBackgroundImagePaths.Value.ToArray(),
				prop => prop.Value = desktops[prop.Index].WallpaperPath);
		}

		private static Action<VirtualDesktop[]> _updateByListAction = new Func<Action<VirtualDesktop[]>>(() =>
		{
			if (ProductInfo.IsWallpaperSupportBuild)
			{
				return desktops =>
				{
					UpdateDesktopNamesCore(desktops);
					UpdateWallpaperPathsCore(desktops);
				};
			}
			else if (ProductInfo.IsNameSupportBuild)
			{
				return UpdateDesktopNamesCore;
			}
			else
			{
				return _ => { };
			}
		})();

		private static void UpdateWindowsDesktopsByList()
		{
			// for OS Build 18975 or later
			var desktops = VirtualDesktop.GetDesktops();
			_updateByListAction(desktops);
		}

		private static void UpdateDesktopNamesCore(VirtualDesktop[] desktops)
		{
			// for OS Build 18975 or later
			var generalSettings = Settings.General;

			var desktopNames = generalSettings.DesktopNames.Value.Select(prop => prop.Value).ToArray();
			for (int i = 0; i < desktopNames.Length; ++i)
			{
				desktops[i].Name = desktopNames[i];
			}
		}

		private static void UpdateWallpaperPathsCore(VirtualDesktop[] desktops)
		{
			// for OS Build 21337 or later
			var generalSettings = Settings.General;

			var wallpaperPaths = generalSettings.DesktopBackgroundImagePaths.Value.Select(prop => prop.Value).ToArray();
			for (int i = 0; i < wallpaperPaths.Length; ++i)
			{
				desktops[i].WallpaperPath = wallpaperPaths[i];
			}
		}

		private static void FitWindowsDesktopsWithList()
		{
			var generalSettings = Settings.General;

			var nameCount = generalSettings.DesktopNames.Count;
			var wallpaperCount = generalSettings.DesktopBackgroundImagePaths.Count;
			var settingsCount = nameCount >= wallpaperCount ? nameCount : wallpaperCount;

			if (nameCount < settingsCount)
			{
				generalSettings.DesktopNames.Resize(settingsCount);
			}
			else if (wallpaperCount < settingsCount)
			{
				generalSettings.DesktopBackgroundImagePaths.Resize(settingsCount);
			}

			var desktops = VirtualDesktop.GetDesktops();
			var currentCount = desktops.Length;

			if (settingsCount > currentCount)
			{
				for (var i = currentCount; i < settingsCount; ++i)
				{
					VirtualDesktop.Create();
				}
			}
			else if (settingsCount < currentCount)
			{
				for (var i = settingsCount; i < currentCount; ++i)
				{
					desktops[i].Remove();
				}
			}
		}

		#endregion
	}
}
