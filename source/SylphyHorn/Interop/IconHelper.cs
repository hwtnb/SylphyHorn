using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using MetroRadiance.Interop;

namespace SylphyHorn.Interop
{
	public static class IconHelper
	{
		public static Icon GetIconFromResource(Uri uri)
		{
			var streamResourceInfo = System.Windows.Application.GetResourceStream(uri);
			if (streamResourceInfo == null) throw new ArgumentException("Resource not found.", nameof(uri));

			using (var stream = streamResourceInfo.Stream)
			{
				var icon = new Icon(stream);

				return ScaleIconToDpi(icon);
			}
		}

		public static RenderTargetBitmap ToRenderTargetBitmap(this System.Windows.Media.DrawingVisual drawingVisual, Size size, Dpi? dpi = null)
		{
			var imageDpi = dpi ?? new Dpi(96, 96);
			var imageSize = ScaleSizeByDpi(size, imageDpi);
			var renderTarget = new RenderTargetBitmap(imageSize.Width, imageSize.Height, imageDpi.X, imageDpi.Y, System.Windows.Media.PixelFormats.Default);
			renderTarget.Render(drawingVisual);
			return renderTarget;
		}

		public static Bitmap ToBitmap(this System.Windows.Media.DrawingVisual drawingVisual, Size size, Dpi? dpi = null, Color? transparentColor = null)
		{
			var renderTarget = drawingVisual.ToRenderTargetBitmap(size, dpi);
			var encoder = new BmpBitmapEncoder();
			var frame = BitmapFrame.Create(renderTarget);
			encoder.Frames.Add(frame);

			using (var stream = new MemoryStream())
			{
				encoder.Save(stream);

				var bitmap = new Bitmap(stream, useIcm: true);
				if (transparentColor.HasValue)
				{
					bitmap.MakeTransparent(transparentColor.Value);
				}
				return bitmap;
			}
		}

		public static Icon ToIcon(this Bitmap bitmap)
		{
			var iconHandle = bitmap.GetHicon();
			var icon = Icon.FromHandle(iconHandle);

			return icon;
		}

#if DEBUG
		internal static BitmapSource ToBitmapSource(this Icon icon)
		{
			using (var bitmap = icon.ToBitmap())
			using (var stream = new MemoryStream())
			{
				bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
				stream.Seek(0, SeekOrigin.Begin);

				var bitmapSource = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
				return bitmapSource;
			}
		}
#endif

		private static Icon ScaleIconToDpi(Icon targetIcon)
		{
			var dpi = GetSystemDpi();

			return new Icon(targetIcon, new Size((int)(16 * dpi.ScaleX), (int)(16 * dpi.ScaleY)));
		}

		public static Size GetIconSize()
		{
			return new Size(
				(int)System.Windows.SystemParameters.SmallIconWidth,
				(int)System.Windows.SystemParameters.SmallIconHeight);
		}

		private static Size ScaleSizeByDpi(Size size, Dpi dpi)
		{
			return new Size(
				(int)Math.Round(size.Width * dpi.ScaleX),
				(int)Math.Round(size.Height * dpi.ScaleY));
		}

		public static Dpi GetSystemDpi()
		{
			return PerMonitorDpi.GetDpi(IntPtr.Zero);
		}
	}
}
