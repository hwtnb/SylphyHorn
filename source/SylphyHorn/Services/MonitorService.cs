using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using MetroRadiance.Interop.Win32;
using SylphyHorn.Interop;
using MONITORINFOEX = SylphyHorn.Interop.MONITORINFOEX;

namespace SylphyHorn.Services
{
	internal class Monitor
	{
		public string Name { get; }

		public Rect MonitorArea { get; }

		public Rect WorkArea { get; }

		public Monitor(string name, Rect monitorArea, Rect workArea)
		{
			this.Name = name;
			this.MonitorArea = monitorArea;
			this.WorkArea = workArea;
		}
	}

	internal static class MonitorService
	{
		public static Monitor[] GetMonitors() => GetMonitorsInternal(true);

		public static Monitor[] GetAreas() => GetMonitorsInternal();

		public static Monitor GetCurrentMonitor() => GetCurrentMonitorInternal(true);

		public static Monitor GetCurrentArea() => GetCurrentMonitorInternal();

		public static IntPtr GetCurrentHMonitor()
		{
			POINT pt;
			if (!NativeMethods.GetCursorPos(out pt)) throw new Win32Exception(Marshal.GetLastWin32Error());

			var hWorkingMonitor = NativeMethods.MonitorFromPoint(pt, MonitorDefaultTo.MONITOR_DEFAULTTONEAREST);
			if (hWorkingMonitor == (IntPtr)MonitorDefaultTo.MONITOR_DEFAULTTONEAREST) throw new InvalidOperationException();

			return hWorkingMonitor;
		}

		private static Monitor[] GetMonitorsInternal(bool additionalRetrive = false)
		{
			var list = new List<Monitor>();
			var ret = NativeMethods.EnumDisplayMonitors(
				IntPtr.Zero,
				IntPtr.Zero,
				(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr _) =>
				{
					var monitor = GetMonitorInternal(hMonitor, additionalRetrive);
					if (monitor != null) list.Add(monitor);
					return true;
				},
				IntPtr.Zero);

			if (!ret) throw new Win32Exception(Marshal.GetLastWin32Error());

			return list.ToArray();
		}

		private static Monitor GetCurrentMonitorInternal(bool additionalRetrive = false)
		{
			var hWorkingMonitor = GetCurrentHMonitor();
			return GetMonitorInternal(hWorkingMonitor, additionalRetrive);
		}

		private static Monitor GetMonitorInternal(IntPtr hMonitor, bool additionalRetrive = false)
		{
			var info = new MONITORINFOEX();
			info.cbSize = Marshal.SizeOf(info);

			if (NativeMethods.GetMonitorInfo(hMonitor, ref info))
			{
				var name = info.szDevice;

				if (additionalRetrive)
				{
					var cPhysicalMonitors = 0;
					if (NativeMethods.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref cPhysicalMonitors))
					{
						var closing = false;
						var numberOfMonitors = 1u;
						var physicalMonitors = new PHYSICAL_MONITOR[numberOfMonitors];
						try
						{
							if (NativeMethods.GetPhysicalMonitorsFromHMONITOR(hMonitor, numberOfMonitors, physicalMonitors))
							{
								name = physicalMonitors[0].szPhysicalMonitorDescription;
								closing = true;
							}
						}
						finally
						{
							if (closing) NativeMethods.DestroyPhysicalMonitors(numberOfMonitors, physicalMonitors);
						}
					}
				}

				return new Monitor(name, info.rcMonitor.ToRect(), info.rcWork.ToRect());
			}

			throw new Win32Exception(Marshal.GetLastWin32Error());
		}

		private static Rect ToRect(this RECT orig)
		{
			return new Rect(
				orig.Left,
				orig.Top,
				orig.Right - orig.Left,
				orig.Bottom - orig.Top);
		}
	}
}
