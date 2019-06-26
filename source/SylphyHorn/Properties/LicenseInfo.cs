using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SylphyHorn.Properties
{
	public class LicenseInfo
	{
		public static LicenseInfo[] All { get; } =
		{
			new LicenseInfo("VirtualDesktop"),
			new LicenseInfo("Open.WinKeyboardHook"),
			new LicenseInfo("InputSimulatorPlus"),
			new LicenseInfo("Livet"),
			new LicenseInfo("StatefulModel"),
			new LicenseInfo("MetroRadiance"),
			new LicenseInfo("MetroTrilithon"),
		};

		public string ProductName { get; }

		public string LicenseBody { get; }

		private LicenseInfo(string productName, string resourceName = null)
		{
			this.ProductName = productName;

			var path = $@"SylphyHorn..licenses.{resourceName ?? $"{productName}.txt"}";
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
			if (stream == null) return;

			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				this.LicenseBody = reader.ReadToEnd();
			}
		}
	}
}
