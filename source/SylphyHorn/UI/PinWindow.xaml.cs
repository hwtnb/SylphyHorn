using System;
using System.Windows;
using System.Windows.Interop;
using MetroRadiance.Interop;
using MetroRadiance.Interop.Win32;
using SylphyHorn.Interop;
using SylphyHorn.Serialization;

namespace SylphyHorn.UI
{
	partial class PinWindow
	{
		private readonly IntPtr _target;

		public PinWindow(IntPtr target)
		{
			this._target = target;
			this.InitializeComponent();
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			RECT rect;
			if (NativeMethods.GetWindowRect(this._target, out rect))
			{
				var targetWidth = rect.Right - rect.Left;
				var targetHeight = rect.Bottom - rect.Top;

				var dpi = PerMonitorDpi.GetDpi(this._target);
				var width = this.ActualWidth * dpi.ScaleX;
				var height = this.ActualHeight * dpi.ScaleY;

				var offsetLeft = Settings.General.NotificationOffsetX;
				var offsetTop = -Settings.General.NotificationOffsetY;

				this.Left = (rect.Left + (targetWidth - width) / 2) / dpi.ScaleX + offsetLeft;
				this.Top = (rect.Top + (targetHeight - height) / 2) / dpi.ScaleY + offsetTop;
			}
		}
	}
}
