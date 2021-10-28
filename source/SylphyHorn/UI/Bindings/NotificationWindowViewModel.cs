using MetroTrilithon.Mvvm;
using SylphyHorn.Serialization;

namespace SylphyHorn.UI.Bindings
{
	public class NotificationWindowViewModel : WindowViewModel
	{
		#region Header 変更通知プロパティ

		private string _Header;

		public string Header
		{
			get { return this._Header; }
			set
			{
				if (this._Header != value)
				{
					this._Header = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region Body 変更通知プロパティ

		private string _Body;

		public string Body
		{
			get { return this._Body; }
			set
			{
				if (this._Body != value)
				{
					this._Body = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region FontFamily 変更通知プロパティ

		public string FontFamily
		{
			get
			{
				var fontFamily = Settings.General.NotificationFontFamily.Value;
				var defaultFont = GeneralSettings.NotificationFontFamilyDefaultValue;
				return !string.IsNullOrEmpty(fontFamily)
					? fontFamily + ", " + defaultFont
					: defaultFont;
			}
		}

		#endregion


	}
}
