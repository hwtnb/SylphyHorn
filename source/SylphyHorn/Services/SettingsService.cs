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
			if (ProductInfo.IsWindows11OrLater)
			{
				var generalSettings = Settings.General;
				overrideDesktops = overrideDesktops && (generalSettings.DesktopNames.Count > 0 || generalSettings.DesktopBackgroundImagePaths.Count > 0);
				if (overrideDesktops)
				{
					FitWindowsDesktopsWithList();
					UpdateWindowsDesktopsByList();
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
			// Only for Window 11
			var desktops = VirtualDesktop.GetDesktops();
			var generalSettings = Settings.General;

			Array.ForEach(generalSettings.DesktopNames.Value.ToArray(),
				prop => prop.Value = desktops[prop.Index].Name);
			Array.ForEach(generalSettings.DesktopBackgroundImagePaths.Value.ToArray(),
				prop => prop.Value = desktops[prop.Index].WallpaperPath);
		}

		#endregion

		#region Resize

		public static void ResizeList()
		{
			var desktopCount = VirtualDesktopService.Count;

			Settings.General.DesktopNames.Resize(desktopCount);
			Settings.General.DesktopBackgroundImagePaths.Resize(desktopCount);
			Settings.General.DesktopBackgroundPositions.Resize(desktopCount);

			Settings.ShortcutKey.SwitchToIndices.Resize(desktopCount);
			Settings.ShortcutKey.MoveToIndices.Resize(desktopCount);
			Settings.ShortcutKey.SwapDesktopIndices.Resize(desktopCount);

			Settings.MouseShortcut.SwitchToIndices.Resize(desktopCount);
			Settings.MouseShortcut.MoveToIndices.Resize(desktopCount);
			Settings.MouseShortcut.SwapDesktopIndices.Resize(desktopCount);
		}

		#endregion

		#region private methods

		private static void UpdateWindowsDesktopsByList()
		{
			// Only for Window 11
			var desktops = VirtualDesktop.GetDesktops();
			var generalSettings = Settings.General;

			var desktopNames = generalSettings.DesktopNames.Value.Select(prop => prop.Value).ToArray();
			var wallpaperPaths = generalSettings.DesktopBackgroundImagePaths.Value.Select(prop => prop.Value).ToArray();

			for (int i = 0; i < desktopNames.Length; ++i)
			{
				desktops[i].Name = desktopNames[i];
			}
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
