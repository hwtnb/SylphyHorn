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

		public SerializableProperty<bool> AlwaysShowDesktopNotification => this.Cache(key => new SerializableProperty<bool>(key, this._provider, AlwaysShowDesktopNotificationDefaultValue));

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

		public SerializableProperty<uint> NotificationHeaderAlignment => this.Cache(key => new SerializableProperty<uint>(key, this._provider, NotificationHeaderAlignmentDefaultValue));

		public SerializableProperty<uint> NotificationBodyAlignment => this.Cache(key => new SerializableProperty<uint>(key, this._provider, NotificationBodyAlignmentDefaultValue));

		public SerializableProperty<string> NotificationFontFamily => this.Cache(key => new SerializableProperty<string>(key, this._provider));

		public SerializableProperty<int> NotificationHeaderFontSize => this.Cache(key => new SerializableProperty<int>(key, this._provider, NotificationHeaderFontSizeDefaultValue));

		public SerializableProperty<int> NotificationBodyFontSize => this.Cache(key => new SerializableProperty<int>(key, this._provider, NotificationBodyFontSizeDefaultValue));

		public SerializableProperty<int> NotificationLineSpacing => this.Cache(key => new SerializableProperty<int>(key, this._provider, NotificationLineSpacingDefaultValue));

		public SerializableProperty<int> NotificationMinWidth => this.Cache(key => new SerializableProperty<int>(key, this._provider, NotificationMinWidthDefaultValue));

		public SerializableProperty<int> SimpleNotificationMinWidth => this.Cache(key => new SerializableProperty<int>(key, this._provider, SimpleNotificationMinWidthDefaultValue));

		public SerializableProperty<int> PinWindowMinWidth => this.Cache(key => new SerializableProperty<int>(key, this._provider, PinWindowMinWidthDefaultValue));

		public SerializableProperty<int> NotificationMinHeight => this.Cache(key => new SerializableProperty<int>(key, this._provider, NotificationMinHeightDefaultValue));

		public SerializableProperty<bool> TrayShowDesktop => this.Cache(key => new SerializableProperty<bool>(key, this._provider, TrayShowDesktopDefaultValue));

		public SerializableProperty<bool> TrayShowOnlyCurrentNumber => this.Cache(key => new SerializableProperty<bool>(key, this._provider, TrayShowOnlyCurrentNumberDefaultValue));

		public SerializableProperty<bool> UseDesktopName => this.Cache(key => new SerializableProperty<bool>(key, this._provider));

		public SerializableProperty<bool> OverrideDesktopsOnStartup => this.Cache(key => new SerializableProperty<bool>(key, this._provider, OverrideDesktopsOnStartupDefaultValue));

		public DesktopNamePropertyList DesktopNames => this.Cache(key => new DesktopNamePropertyList(key, this._provider));

		public WallpaperPathPropertyList DesktopBackgroundImagePaths => this.Cache(key => new WallpaperPathPropertyList(key, this._provider));

		public WallpaperPositionsPropertyList DesktopBackgroundPositions => this.Cache(key => new WallpaperPositionsPropertyList(key, this._provider));

		#region default values

		public static bool NotificationWhenSwitchedDesktopDefaultValue { get; } = true;

		public static bool AlwaysShowDesktopNotificationDefaultValue { get; } = false;

		public static int NotificationDurationDefaultValue { get; } = 2500 /* milliseconds */;

		public static bool OverrideWindowsDefaultKeyCombinationDefaultValue { get; } = false;

		public static uint PlacementDefaultValue { get; } = 5 /* Center */;

		public static uint NotificationWindowStyleDefaultValue { get; } = 4 /* BlurWindowThemeMode.System */;

		public static uint NotificationHeaderAlignmentDefaultValue { get; } = 0 /* Left */;

		public static uint NotificationBodyAlignmentDefaultValue { get; } = 0 /* Left */;

		public static int NotificationHeaderFontSizeDefaultValue { get; } = 18 /* px */;

		public static int NotificationBodyFontSizeDefaultValue { get; } = 32 /* px */;

		public static int NotificationLineSpacingDefaultValue { get; } = -4;

		public static int NotificationMinWidthDefaultValue { get; } = 500 /* px */;

		public static int SimpleNotificationMinWidthDefaultValue { get; } = 210 /* px */;

		public static int PinWindowMinWidthDefaultValue { get; } = 400 /* px */;

		public static int NotificationMinHeightDefaultValue { get; } = 100 /* px */;

		public static string NotificationFontFamilyDefaultValue { get; } = "Segoe UI Light, Yu Gothic UI Light, Meiryo UI";

		public static bool TrayShowDesktopDefaultValue { get; } = false;

		public static bool TrayShowOnlyCurrentNumberDefaultValue { get; } = false;

		public static bool OverrideDesktopsOnStartupDefaultValue { get; } = false;

		#endregion
	}
}
