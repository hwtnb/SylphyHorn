using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SylphyHorn.Services
{
	public sealed class JpegXrSupportDetector : ClsidImageFormatSupportDetector
	{
		public override string[] Extensions => new string[] { ".wdp", ".jxr" };

		public override string FileType => "JPEG XR";

		public override Guid CLSID => new Guid(0xa26cec36, 0x234c, 0x4950, 0xae, 0x16, 0xe3, 0x4a, 0xac, 0xe7, 0x1d, 0x0d);
	}

	public sealed class WebPSupportDetector : ClsidImageFormatSupportDetector
	{
		public override string[] Extensions => new string[] { ".webp" };

		public override string FileType => "WebP";

		public override Guid CLSID => new Guid(0x7693e886, 0x51c9, 0x4070, 0x84, 0x19, 0x9f, 0x70, 0x73, 0x8e, 0xc8, 0xfa);
	}

	public sealed class HEIFSupportDetector : ClsidImageFormatSupportDetector
	{
		private bool? _isHIFExtensionSupported;

		public bool IsHIFExtensionSupported
		{
			get
			{
				if (!this._isHIFExtensionSupported.HasValue)
				{
					if (Environment.OSVersion.Version.Build >= 21301)
					{
						this._isHIFExtensionSupported = true;
					}
					else if (Environment.OSVersion.Version.Build >= 19041)
					{
						const string targetHootfixId = "KB5003214";
						const string query = "SELECT HotFixID FROM Win32_QuickFixEngineering";

						bool supported = false;
						var searcher = new System.Management.ManagementObjectSearcher(query);
						foreach (var hotfix in searcher.Get())
						{
							if (hotfix["HotFixID"].ToString() == targetHootfixId)
							{
								supported = true;
								break;
							}
						}
						this._isHIFExtensionSupported = supported;
					}
					else
					{
						this._isHIFExtensionSupported = false;
					}
				}
				return this._isHIFExtensionSupported.Value;
			}
		}

		private bool? _isHEVCSupported;

		public bool IsHEVCSupported
		{
			get
			{
				if (!this._isHEVCSupported.HasValue)
				{
					const string targetKeyName = "Microsoft.HEVCVideoExtension_";

					this._isHEVCSupported = Registry.ClassesRoot
						.OpenSubKey("ActivatableClasses")
						.OpenSubKey("Package")
						.GetSubKeyNames()
						.Any(name => name.StartsWith(targetKeyName));
				}
				return this._isHEVCSupported.Value;
			}
		}

		private bool? _isAV1Supported;

		public bool IsAV1Supported
		{
			get
			{
				if (!this._isAV1Supported.HasValue)
				{
					const string targetKeyName = "Microsoft.AV1VideoExtension_";

					this._isAV1Supported = Registry.ClassesRoot
						.OpenSubKey("ActivatableClasses")
						.OpenSubKey("Package")
						.GetSubKeyNames()
						.Any(name => name.StartsWith(targetKeyName));
				}
				return this._isAV1Supported.Value;
			}
		}

		private bool? _isAVIFSupported;

		public bool IsAVIFSupported
		{
			get
			{
				if (!this._isAVIFSupported.HasValue)
				{
					this._isAVIFSupported = Environment.OSVersion.Version.Build >= 18305 && this.IsAV1Supported;
				}
				return this._isAVIFSupported.Value;
			}
		}

		public override string[] Extensions
		{
			get
			{
				var extensions = new Collection<string>();
				if (this.IsHIFExtensionSupported)
				{
					extensions.Add(".hif");
				}
				extensions.Add(".heif");
				extensions.Add(".heifs");
				extensions.Add(".avci");
				extensions.Add(".avcs");
				if (this.IsHEVCSupported)
				{
					extensions.Add(".heic");
					extensions.Add(".heics");
				}
				if (this.IsAVIFSupported)
				{
					extensions.Add(".avif");
					extensions.Add(".avifs");
				}
				return extensions.ToArray();
			}
		}

		public override string FileType
		{
			get
			{
				return this.IsAVIFSupported
					? this.IsHEVCSupported
						? "HEIF (AVCI, HEIC, AVIF)"
						: "HEIF (AVCI, AVIF)"
					: this.IsHEVCSupported
						? "HEIF (AVCI, HEIC)"
						: "HEIF (AVCI)";
			}
		}

		public override Guid CLSID => new Guid(0xe9a4a80a, 0x44fe, 0x4de4, 0x89, 0x71, 0x71, 0x50, 0xb1, 0x0a, 0x51, 0x99);
	}
}
