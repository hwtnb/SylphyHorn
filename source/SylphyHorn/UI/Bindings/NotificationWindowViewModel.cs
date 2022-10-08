using System.Windows;
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

		#region FontSize 変更通知プロパティ

		public int HeaderFontSize => Settings.General.NotificationHeaderFontSize;

		public int BodyFontSize => Settings.General.NotificationBodyFontSize;

		#endregion

		#region Margin 変更通知プロパティ

		public string HeaderMargin
		{
			get
			{
				var alignment = (HorizontalAlignment)Settings.General.NotificationHeaderAlignment.Value;
				var lineSpacing = Settings.General.NotificationLineSpacing.Value;
				if (alignment == HorizontalAlignment.Left)
				{
					return $"2,0,0,{lineSpacing}";
				}
				else if (alignment == HorizontalAlignment.Right)
				{
					return $"0,0,6,{lineSpacing}";
				}
				else
				{
					return $"0,0,0,{lineSpacing}";
				}
			}
		}

		public string BodyMargin => Settings.General.SimpleNotification.Value ? "0,-4,4,0" : "0,0,4,0";

		#endregion

		#region Visibility 変更通知プロパティ

		public Visibility HeaderVisibility => string.IsNullOrEmpty(this.Header) ? Visibility.Collapsed : Visibility.Visible;

		public Visibility BodyVisibility => Visibility.Visible;

		#endregion

		#region Alignment 変更通知プロパティ

		public string HeaderAlignment => ((HorizontalAlignment)Settings.General.NotificationHeaderAlignment.Value).ToString();

		public string BodyAlignment => ((HorizontalAlignment)Settings.General.NotificationBodyAlignment.Value).ToString();

		#endregion

		#region WindowMinSize 変更通知プロパティ

		public int WindowMinWidth => Settings.General.SimpleNotification ? Settings.General.SimpleNotificationMinWidth : Settings.General.NotificationMinWidth;

		public int PinWindowMinWidth => Settings.General.PinWindowMinWidth;

		public int WindowMinHeight => Settings.General.NotificationMinHeight;

		#endregion
	}
}
