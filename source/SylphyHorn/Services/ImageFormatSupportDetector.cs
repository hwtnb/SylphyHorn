using System;
using System.Linq;
using System.Runtime.InteropServices;
using WindowsDesktop.Interop;

namespace SylphyHorn.Services
{
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
				var extensionInfo = String.Join(";", this.Extensions.Select(e => $"*{e}"));
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
