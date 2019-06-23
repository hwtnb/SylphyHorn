using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SylphyHorn.Serialization
{
	public static class Settings
	{
		public static GeneralSettings General { get; } = new GeneralSettings(LocalSettingsProvider.Instance);

		public static ShortcutKeySettings ShortcutKey { get; } = new ShortcutKeySettings(LocalSettingsProvider.Instance);
		public static MouseShortcutSettings MouseShortcut { get; } = new MouseShortcutSettings(LocalSettingsProvider.Instance);
	}
}
