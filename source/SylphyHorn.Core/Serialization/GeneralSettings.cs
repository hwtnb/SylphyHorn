using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetroTrilithon.Serialization;

namespace SylphyHorn.Serialization
{
	public class GeneralSettings : SettingsHost
	{
		private readonly ISerializationProvider _provider;

		public GeneralSettings(ISerializationProvider provider)
		{
			this._provider = provider;
		}

		public SerializableProperty<bool> LoopDesktop => this.Cache(key => new SerializableProperty<bool>(key, this._provider));

		public SerializableProperty<bool> NotificationWhenSwitchedDesktop => this.Cache(key => new SerializableProperty<bool>(key, this._provider, NotificationWhenSwitchedDesktopDefaultValue));

		public SerializableProperty<bool> SimpleNotification => this.Cache(key => new SerializableProperty<bool>(key, this._provider));

		public SerializableProperty<int> NotificationDuration => this.Cache(key => new SerializableProperty<int>(key, this._provider, NotificationDurationDefaultValue));

		public SerializableProperty<bool> ChangeBackgroundEachDesktop => this.Cache(key => new SerializableProperty<bool>(key, this._provider));

		public SerializableProperty<string> DesktopBackgroundFolderPath => this.Cache(key => new SerializableProperty<string>(key, this._provider));

		public SerializableProperty<bool> OverrideWindowsDefaultKeyCombination => this.Cache(key => new SerializableProperty<bool>(key, this._provider, OverrideWindowsDefaultKeyCombinationDefaultValue));

		public SerializableProperty<bool> SuspendKeyDetection => this.Cache(key => new SerializableProperty<bool>(key, this._provider));

		public SerializableProperty<bool> FirstTime => this.Cache(key => new SerializableProperty<bool>(key, this._provider, true));

		public SerializableProperty<string> Culture => this.Cache(key => new SerializableProperty<string>(key, this._provider));

		public SerializableProperty<uint> Placement => this.Cache(key => new SerializableProperty<uint>(key, this._provider, PlacementDefaultValue));

		public SerializableProperty<uint> Display => this.Cache(key => new SerializableProperty<uint>(key, this._provider, 0));

		public SerializableProperty<uint> NotificationWindowStyle => this.Cache(key => new SerializableProperty<uint>(key, this._provider, NotificationWindowStyleDefaultValue));

		public SerializableProperty<string> NotificationFontFamily => this.Cache(key => new SerializableProperty<string>(key, this._provider));

		public SerializableProperty<bool> TrayShowDesktop => this.Cache(key => new SerializableProperty<bool>(key, this._provider, TrayShowDesktopDefaultValue));

		public SerializableProperty<bool> UseDesktopName => this.Cache(key => new SerializableProperty<bool>(key, this._provider));

		public SerializableProperty<bool> OverrideDesktopsOnStartup => this.Cache(key => new SerializableProperty<bool>(key, this._provider, OverrideDesktopsOnStartupDefaultValue));

		public DesktopNamePropertyList DesktopNames => this.Cache(key => new DesktopNamePropertyList(key, this._provider));

		public WallpaperPathPropertyList DesktopBackgroundImagePaths => this.Cache(key => new WallpaperPathPropertyList(key, this._provider));

		public WallpaperPositionsPropertyList DesktopBackgroundPositions => this.Cache(key => new WallpaperPositionsPropertyList(key, this._provider));

		#region default values

		public static bool NotificationWhenSwitchedDesktopDefaultValue { get; } = true;

		public static int NotificationDurationDefaultValue { get; } = 2500 /* milliseconds */;

		public static bool OverrideWindowsDefaultKeyCombinationDefaultValue { get; } = false;

		public static uint PlacementDefaultValue { get; } = 5 /* Center */;

		public static uint NotificationWindowStyleDefaultValue { get; } = 4 /* BlurWindowThemeMode.System */;

		public static string NotificationFontFamilyDefaultValue { get; } = "Segoe UI Light, Yu Gothic UI Light, Meiryo UI";

		public static bool TrayShowDesktopDefaultValue { get; } = false;

		public static bool OverrideDesktopsOnStartupDefaultValue { get; } = false;

		#endregion
	}
}
