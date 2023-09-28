using System;
using System.Windows;
using System.Windows.Interop;
using MetroRadiance.Interop;
using SylphyHorn.Serialization;
using SylphyHorn.UI.Bindings;

namespace SylphyHorn.UI
{
	partial class SwitchWindow
	{
		private readonly Rect _area;

		public SwitchWindow(Rect area)
		{
			this._area = area;
			this.InitializeComponent();
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			var source = PresentationSource.FromVisual(this) as HwndSource;
			if (source == null) throw new InvalidOperationException();

			var dpi = source.GetDpi();
			var width = this.ActualWidth * dpi.ScaleX;
			var height = this.ActualHeight * dpi.ScaleY;
			var area = this._area;

			var offsetLeft = Settings.General.NotificationOffsetX;
			var offsetTop = -Settings.General.NotificationOffsetY;

			switch ((WindowPlacement)Settings.General.Placement.Value)
			{
				case WindowPlacement.TopLeft:
				case WindowPlacement.CenterLeft:
				case WindowPlacement.BottomLeft:
					this.Left = area.Left / dpi.ScaleX + offsetLeft;
					break;

				case WindowPlacement.TopRight:
				case WindowPlacement.CenterRight:
				case WindowPlacement.BottomRight:
					this.Left = (area.Right - width) / dpi.ScaleX + offsetLeft;
					break;

				case WindowPlacement.Center:
				default:
					this.Left = (area.Left + (area.Width - width) / 2) / dpi.ScaleX + offsetLeft;
					break;
			}

			switch ((WindowPlacement)Settings.General.Placement.Value)
			{
				case WindowPlacement.TopLeft:
				case WindowPlacement.TopCenter:
				case WindowPlacement.TopRight:
					this.Top = area.Top / dpi.ScaleY + offsetTop;
					break;

				case WindowPlacement.BottomLeft:
				case WindowPlacement.BottomCenter:
				case WindowPlacement.BottomRight:
					this.Top = (area.Bottom - height) / dpi.ScaleY + offsetTop;
					break;

				case WindowPlacement.Center:
				default:
					this.Top = (area.Top + (area.Height - height) / 2) / dpi.ScaleY + offsetTop;
					break;
			}
		}
	}
}
