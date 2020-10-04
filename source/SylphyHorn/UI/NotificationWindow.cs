using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using MetroRadiance.Interop.Win32;
using MetroRadiance.UI.Controls;
using SylphyHorn.Interop;
using SylphyHorn.Serialization;

namespace SylphyHorn.UI
{
	public class NotificationWindow : BlurWindow
	{
		#region NativeOpacity dependency property

		public static readonly DependencyProperty NativeOpacityProperty = DependencyProperty.Register(
			nameof(NativeOpacity), typeof(double), typeof(NotificationWindow), new PropertyMetadata(default(double), HandleNativeOpacityChanged));

		public double NativeOpacity
		{
			get { return (double)this.GetValue(NativeOpacityProperty); }
			set { this.SetValue(NativeOpacityProperty, value); }
		}

		private static void HandleNativeOpacityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			((NotificationWindow)sender).ChangeOpacity((double)args.NewValue);
		}

		#endregion

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			var settings = Settings.General.NotificationWindowStyle.Value;
			this.ThemeMode = (BlurWindowThemeMode)settings;

			var source = PresentationSource.FromVisual(this) as HwndSource;
			if (source == null) throw new InvalidOperationException();

			var style = User32.GetWindowLongEx(source.Handle);
			style |= WindowExStyles.WS_EX_TOOLWINDOW | WindowExStyles.WS_EX_NOACTIVATE;
			User32.SetWindowLongEx(source.Handle, style);
		}

		protected override void OnThemeModeChanged(DependencyPropertyChangedEventArgs e)
		{
			var settings = Settings.General.NotificationWindowStyle.Value;
			this.ThemeMode = (BlurWindowThemeMode)settings;
		}

		private void ChangeOpacity(double opacity)
		{
			var source = (HwndSource)PresentationSource.FromVisual(this);
			if (source == null) return;

			var bAlpha = (byte)(opacity * 255.0);
			NativeMethods.SetLayeredWindowAttributes(source.Handle, 0, bAlpha, LayeredWindowAttributes.Alpha);
		}
	}
}
