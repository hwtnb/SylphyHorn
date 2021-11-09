using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using WindowsDesktop.Interop;

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

	public sealed class HeifSupportDetector : ClsidImageFormatSupportDetector
	{
		private bool? _isHifExtensionSupported;

		public bool IsHifExtensionSupported
		{
			get
			{
				if (!this._isHifExtensionSupported.HasValue)
				{
					if (Environment.OSVersion.Version.Build >= 21301)
					{
						this._isHifExtensionSupported = true;
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
						this._isHifExtensionSupported = supported;
					}
					else
					{
						this._isHifExtensionSupported = false;
					}
				}
				return this._isHifExtensionSupported.Value;
			}
		}

		private bool? _isHevcSupported;

		public bool IsHevcSupported
		{
			get
			{
				if (!this._isHevcSupported.HasValue)
				{
					const string targetKeyName = "Microsoft.HEVCVideoExtension_";

					this._isHevcSupported = Registry.ClassesRoot
						.OpenSubKey("ActivatableClasses")
						.OpenSubKey("Package")
						.GetSubKeyNames()
						.Any(name => name.StartsWith(targetKeyName));
				}
				return this._isHevcSupported.Value;
			}
		}

		private bool? _isAv1Supported;

		public bool IsAV1Supported
		{
			get
			{
				if (!this._isAv1Supported.HasValue)
				{
					const string targetKeyName = "Microsoft.AV1VideoExtension_";

					this._isAv1Supported = Registry.ClassesRoot
						.OpenSubKey("ActivatableClasses")
						.OpenSubKey("Package")
						.GetSubKeyNames()
						.Any(name => name.StartsWith(targetKeyName));
				}
				return this._isAv1Supported.Value;
			}
		}

		private bool? _isAvifSupported;

		public bool IsAVIFSupported
		{
			get
			{
				if (!this._isAvifSupported.HasValue)
				{
					this._isAvifSupported = Environment.OSVersion.Version.Build >= 18305 && this.IsAV1Supported;
				}
				return this._isAvifSupported.Value;
			}
		}

		public override string[] Extensions
		{
			get
			{
				var extensions = new Collection<string>();
				if (this.IsHifExtensionSupported)
				{
					extensions.Add(".hif");
				}
				extensions.Add(".heif");
				extensions.Add(".heifs");
				extensions.Add(".avci");
				extensions.Add(".avcs");
				if (this.IsHevcSupported)
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
					? this.IsHevcSupported
						? "HEIF (AVCI, HEIC, AVIF)"
						: "HEIF (AVCI, AVIF)"
					: this.IsHevcSupported
						? "HEIF (AVCI, HEIC)"
						: "HEIF (AVCI)";
			}
		}

		public override Guid CLSID => new Guid(0xe9a4a80a, 0x44fe, 0x4de4, 0x89, 0x71, 0x71, 0x50, 0xb1, 0x0a, 0x51, 0x99);
	}

	public abstract class ImageFormatSupportDetector
	{
		private bool? _isSupported = null;

		public bool IsSupported
		{
			get
			{
				if (!this._isSupported.HasValue)
				{
					this._isSupported = this.GetValue();
				}
				return this._isSupported.Value;
			}
		}

		public string FormatInfo
		{
			get
			{
				var extensionInfo = string.Join(";", this.Extensions.Select(e => $"*{e}"));
				return $"{this.FileType} ({extensionInfo})|{extensionInfo}";
			}
		}

		public abstract string[] Extensions { get; }

		public abstract string FileType { get; }

		public abstract bool GetValue();
	}

	public abstract class ClsidImageFormatSupportDetector : ImageFormatSupportDetector
	{
		public abstract Guid CLSID { get; }

		public override bool GetValue()
		{
			try
			{
				var decoderType = Type.GetTypeFromCLSID(this.CLSID);
				var decoder = Activator.CreateInstance(decoderType);
				return true;
			}
			catch (COMException ex) when (ex.Match(HResult.REGDB_E_CLASSNOTREG))
			{
				return false;
			}
		}
	}
}
