using System.Windows.Media;

namespace SylphyHorn.Interop
{
	public static class ColorHelper
	{
		public static System.Drawing.Color ToGDIColor(this Color color)
		{
			return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
		}
	}
}
