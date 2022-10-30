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
					SuspendResizing();
					FitWindowsDesktopsWithList();
					UpdateWindowsDesktopsByList();
					ResumeResizing();
					StretchShortcutListTo(VirtualDesktopService.Count);
				}
				else
				{
					ResizeListIfNeeded();
					SynchronizeWithWindows();
				}
			}
			else
			{
				ResizeListIfNeeded();
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
			var desktops = VirtualDesktop.AllDesktops;

			_synchronizeWithWindowsAction(desktops);
		}

		#endregion

		#region Resize

		[Obsolete]
		public static void ResizeList()
		{
			if (!_allowResize) return;

			var desktopCount = VirtualDesktopService.Count;

			ResizeDesktopListCore(desktopCount);
			ResizeShortcutListCore(desktopCount);
		}

		public static void ResizeListIfNeeded()
		{
			if (!_allowResize) return;

			var desktopCount = VirtualDesktopService.Count;

			ResizeDesktopListCore(desktopCount);
			ResizeShortcutListIfEmptyCore(desktopCount);
		}

		[Obsolete]
		public static void ResizeShortcutList()
		{
			if (!_allowResize) return;

			var desktopCount = VirtualDesktopService.Count;

			ResizeShortcutListCore(desktopCount);
		}

		public static void StretchShortcutListTo(int count)
		{
			if (!_allowResize) return;

			StretchShortcutListCore(count);
		}

		#endregion

		#region private methods

		private static bool _allowResize = true;

		private static void SuspendResizing() => _allowResize = false;

		private static void ResumeResizing() => _allowResize = true;

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
			Settings.ShortcutKey.MoveToIndicesAndSwitch.Resize(desktopCount);
			Settings.ShortcutKey.SwapDesktopIndices.Resize(desktopCount);

			Settings.MouseShortcut.SwitchToIndices.Resize(desktopCount);
			Settings.MouseShortcut.MoveToIndices.Resize(desktopCount);
			Settings.MouseShortcut.MoveToIndicesAndSwitch.Resize(desktopCount);
			Settings.MouseShortcut.SwapDesktopIndices.Resize(desktopCount);
		}

		private static void ResizeShortcutListIfEmptyCore(int desktopCount)
		{
			Settings.ShortcutKey.SwitchToIndices.ResizeIfEmpty(desktopCount);
			Settings.ShortcutKey.MoveToIndices.ResizeIfEmpty(desktopCount);
			Settings.ShortcutKey.MoveToIndicesAndSwitch.ResizeIfEmpty(desktopCount);
			Settings.ShortcutKey.SwapDesktopIndices.ResizeIfEmpty(desktopCount);

			Settings.MouseShortcut.SwitchToIndices.ResizeIfEmpty(desktopCount);
			Settings.MouseShortcut.MoveToIndices.ResizeIfEmpty(desktopCount);
			Settings.MouseShortcut.MoveToIndicesAndSwitch.ResizeIfEmpty(desktopCount);
			Settings.MouseShortcut.SwapDesktopIndices.ResizeIfEmpty(desktopCount);
		}

		private static void StretchShortcutListCore(int count)
		{
			Settings.ShortcutKey.SwitchToIndices.StretchTo(count);
			Settings.ShortcutKey.MoveToIndices.StretchTo(count);
			Settings.ShortcutKey.MoveToIndicesAndSwitch.StretchTo(count);

			Settings.MouseShortcut.SwitchToIndices.StretchTo(count);
			Settings.MouseShortcut.MoveToIndices.StretchTo(count);
			Settings.MouseShortcut.MoveToIndicesAndSwitch.StretchTo(count);

			if (ProductInfo.IsReorderingSupportBuild)
			{
				Settings.ShortcutKey.SwapDesktopIndices.StretchTo(count);
				Settings.MouseShortcut.SwapDesktopIndices.StretchTo(count);
			}
			else
			{
				Settings.ShortcutKey.SwapDesktopIndices.Resize(count);
				Settings.MouseShortcut.SwapDesktopIndices.Resize(count);
			}
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
				return desktops =>
				{
					UpdateDesktopNamesCore(desktops);
					UpdateWallpaperPathsCoreForWin10();
				};
			}
			else
			{
				return _ => UpdateWallpaperPathsCoreForWin10();
			}
		})();

		private static void UpdateWindowsDesktopsByList()
		{
			// for OS Build 18975 or later
			var desktops = VirtualDesktop.AllDesktops;
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

		private static void UpdateWallpaperPathsCoreForWin10()
		{
			if (!Settings.General.ChangeBackgroundEachDesktop) return;

			WallpaperService.SetWallpaperAndPosition(VirtualDesktop.Current);
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
			if (generalSettings.DesktopBackgroundPositions.Count < settingsCount)
			{
				generalSettings.DesktopBackgroundPositions.Resize(settingsCount);
			}

			var desktops = VirtualDesktop.AllDesktops;
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
