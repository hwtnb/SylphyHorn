using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SylphyHorn.Services;

#if WINDOWS_UWP
using Windows.System;
#else
using VirtualKey = System.Windows.Forms.Keys;
#endif

namespace SylphyHorn.Serialization
{
	public static class SerializationExtensions
	{
		public static ShortcutKey ToShortcutKey(this ShortcutkeyProperty property)
		{
			return property?.Value == null 
				? ShortcutKey.None 
				: ToShortcutKey(property.Value);
		}

		public static ShortcutKey ToShortcutKey(this IList<int> keyCodes)
		{
			if (keyCodes == null) return ShortcutKey.None;

			var key = keyCodes.Count >= 1 ? (VirtualKey)keyCodes[0] : VirtualKey.None;
			var modifiers = keyCodes.Count >= 2 ? keyCodes.Skip(1).Select(x => (VirtualKey)x).ToArray() : Array.Empty<VirtualKey>();
			var result = new ShortcutKey(key, modifiers);

			return result;
		}

		public static IList<int> ToSerializable(this ShortcutKey shortcutKey)
		{
			if (shortcutKey.Key == VirtualKey.None) return Array.Empty<int>();

			var key = new List<int> { (int)shortcutKey.Key, };

			return shortcutKey.Modifiers.Length == 0
				? key
				: key.Concat(shortcutKey.Modifiers.Select(x => (int)x)).ToList();
		}

		public static WallpaperPathProperty InitializeIfEmpty(this WallpaperPathProperty path)
		{
			if (path == null) return path;

			if (string.IsNullOrEmpty(path.Value))
			{
				var currentPath = WallpaperService.GetCurrentColorAndWallpaper().Item2;
				if (string.IsNullOrEmpty(currentPath))
				{
					currentPath = Settings.General.DesktopBackgroundImagePaths.Value
						.FirstOrDefault(p => !string.IsNullOrEmpty(p));
				}
				path.Value = currentPath ?? "";
			}

			return path;
		}
	}
}
