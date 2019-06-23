using MetroRadiance.Interop.Win32;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace SylphyHorn.Interop
{
	[StructLayout(LayoutKind.Sequential)]
	public struct MSLLHOOKSTRUCT
	{
		public POINT pt;
		public uint mouseData;
		public uint flags;
		public uint time;
		public System.IntPtr dwExtraInfo;
	}
}
