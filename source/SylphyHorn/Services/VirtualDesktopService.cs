using System;
using System.Linq;
using System.Media;
using SylphyHorn.Serialization;
using WindowsDesktop;
using GregsStack.InputSimulatorStandard;
using GregsStack.InputSimulatorStandard.Native;

namespace SylphyHorn.Services
{
	internal static class VirtualDesktopService
	{
		#region Count

		public static int Count => VirtualDesktop.GetDesktops().Length;

		#endregion

		#region Get

		public static VirtualDesktop GetLeft()
		{
			var current = VirtualDesktop.Current;
			var desktops = VirtualDesktop.GetDesktops();

			return desktops.Length >= 2 && current.Id == desktops.First().Id
				? Settings.General.LoopDesktop ? desktops.Last() : null
				: current.GetLeft();
		}

		public static VirtualDesktop GetRight()
		{
			var current = VirtualDesktop.Current;
			var desktops = VirtualDesktop.GetDesktops();

			return desktops.Length >= 2 && current.Id == desktops.Last().Id
				? Settings.General.LoopDesktop ? desktops.First() : null
				: current.GetRight();
		}

		public static VirtualDesktop GetByIndex(int index)
		{
			var desktops = VirtualDesktop.GetDesktops();

			return (index >= 0) && (index < desktops.Length) ? desktops[index] : null;
		}

		#endregion

		#region Move Window

		public static VirtualDesktop MoveToLeft(this IntPtr hWnd)
		{
			var current = VirtualDesktop.FromHwnd(hWnd);
			if (current != null)
			{
				var left = current.GetLeft();
				if (left == null)
				{
					if (Settings.General.LoopDesktop)
					{
						var desktops = VirtualDesktop.GetDesktops();
						if (desktops.Length >= 2) left = desktops.Last();
					}
				}
				if (left != null)
				{
					VirtualDesktopHelper.MoveToDesktop(hWnd, left);
					return left;
				}
			}

			SystemSounds.Asterisk.Play();
			return null;
		}

		public static VirtualDesktop MoveToRight(this IntPtr hWnd)
		{
			var current = VirtualDesktop.FromHwnd(hWnd);
			if (current != null)
			{
				var right = current.GetRight();
				if (right == null)
				{
					if (Settings.General.LoopDesktop)
					{
						var desktops = VirtualDesktop.GetDesktops();
						if (desktops.Length >= 2) right = desktops.First();
					}
				}
				if (right != null)
				{
					VirtualDesktopHelper.MoveToDesktop(hWnd, right);
					return right;
				}
			}

			SystemSounds.Asterisk.Play();
			return null;
		}

		public static VirtualDesktop MoveToIndex(this IntPtr hWnd, int i)
		{
			var current = VirtualDesktop.FromHwnd(hWnd);
			if (current != null)
			{
				var target = GetByIndex(i);
				if (target != null)
				{
					VirtualDesktopHelper.MoveToDesktop(hWnd, target);
					return target;
				}
			}

			SystemSounds.Asterisk.Play();
			return null;
		}

		public static VirtualDesktop MoveToNew(this IntPtr hWnd)
		{
			var newone = VirtualDesktop.Create();
			if (newone != null)
			{
				VirtualDesktopHelper.MoveToDesktop(hWnd, newone);
				return newone;
			}

			SystemSounds.Asterisk.Play();
			return null;
		}

		#endregion

		#region Move Desktop

		public static VirtualDesktop MoveToLeft(this VirtualDesktop current)
		{
			if (current != null)
			{
				var left = current.GetLeft();
				if (left == null)
				{
					if (Settings.General.LoopDesktop)
					{
						var desktops = VirtualDesktop.GetDesktops();
						if (desktops.Length >= 2) current.Move(desktops.Length - 1);
					}
				}
				else
				{
					current.Move(left.Index);
				}
				return current;
			}

			SystemSounds.Asterisk.Play();
			return null;
		}

		public static VirtualDesktop MoveToRight(this VirtualDesktop current)
		{
			if (current != null)
			{
				var right = current.GetRight();
				if (right == null)
				{
					if (Settings.General.LoopDesktop)
					{
						var desktops = VirtualDesktop.GetDesktops();
						if (desktops.Length >= 2) current.Move(0);
					}
				}
				else
				{
					current.Move(right.Index);
				}
				return current;
			}

			SystemSounds.Asterisk.Play();
			return null;
		}

		public static VirtualDesktop MoveToFirst(this VirtualDesktop current)
		{
			if (current != null && Count > 0)
			{
				current.Move(0);
				return current;
			}

			SystemSounds.Asterisk.Play();
			return null;
		}

		public static VirtualDesktop MoveToLast(this VirtualDesktop current)
		{
			var desktopCount = Count;
			if (current != null && desktopCount > 0)
			{
				current.Move(desktopCount - 1);
				return current;
			}

			SystemSounds.Asterisk.Play();
			return null;
		}

		public static VirtualDesktop MoveToIndex(this VirtualDesktop current, int i)
		{
			if (current != null && 0 <= i && i < Count)
			{
				current.Move(i);
				return current;
			}

			SystemSounds.Asterisk.Play();
			return null;
		}

		#endregion

		#region Swap Desktop

		public static VirtualDesktop SwapCurrentForLeft()
		{
			var current = VirtualDesktop.Current;

			return current.MoveToLeft();
		}

		public static VirtualDesktop SwapCurrentForRight()
		{
			var current = VirtualDesktop.Current;

			return current.MoveToRight();
		}

		public static VirtualDesktop SwapCurrentForFirst()
		{
			var current = VirtualDesktop.Current;

			return current.MoveToFirst();
		}

		public static VirtualDesktop SwapCurrentForLast()
		{
			var current = VirtualDesktop.Current;

			return current.MoveToLast();
		}

		public static VirtualDesktop SwapCurrentByIndex(int index)
		{
			var current = VirtualDesktop.Current;

			return current.MoveToIndex(index);
		}

		public static Tuple<VirtualDesktop, VirtualDesktop> SwapDesktops(int index1, int index2)
		{
			if (index1 >= 0 && index2 >= 0)
			{
				var desktops = VirtualDesktop.GetDesktops();
				var desktopCount = desktops.Length;
			
				if (index1 < desktopCount && index2 < desktopCount)
				{
					var desktop1 = desktops[index1];
					var desktop2 = desktops[index2];
					desktop1.Move(index2);
					desktop2.Move(index1);
					return new Tuple<VirtualDesktop, VirtualDesktop>(desktop1, desktop2);
				}
			}

			return new Tuple<VirtualDesktop, VirtualDesktop>(null, null);
		}

		#endregion

		#region Create

		public static void CreateAndSwitch()
		{
			VirtualDesktop.Create()?.Switch();
		}

		#endregion

		#region Close

		public static void CloseAndSwitchLeft()
		{
			var current = VirtualDesktop.Current;
			var desktops = VirtualDesktop.GetDesktops();
			
			if (desktops.Length > 1)
			{
				GetLeft()?.Switch();
				current.Remove();
			}
		}

		public static void CloseAndSwitchRight()
		{
			var current = VirtualDesktop.Current;
			var desktops = VirtualDesktop.GetDesktops();

			if (desktops.Length > 1)
			{
				GetRight()?.Switch();
				current.Remove();
			}
		}

		#endregion

		#region Task View

		private static readonly InputSimulator Input = new InputSimulator();

		public static void ShowTaskView()
		{
			Input.Keyboard
				.KeyUp(VirtualKeyCode.CONTROL)
				.KeyUp(VirtualKeyCode.SHIFT)
				.KeyUp(VirtualKeyCode.MENU)
				.KeyUp(VirtualKeyCode.LWIN)
				.KeyUp(VirtualKeyCode.RWIN)
				.ModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.TAB);
		}

		public static void ShowWindowSwitch()
		{
			Input.Keyboard
				.KeyUp(VirtualKeyCode.CONTROL)
				.KeyUp(VirtualKeyCode.SHIFT)
				.KeyUp(VirtualKeyCode.MENU)
				.KeyUp(VirtualKeyCode.LWIN)
				.KeyUp(VirtualKeyCode.RWIN)
				.ModifiedKeyStroke(new[] {VirtualKeyCode.CONTROL, VirtualKeyCode.MENU}, VirtualKeyCode.TAB);
		}

		#endregion

		#region Pin / Unpin

		public static event EventHandler<WindowPinnedEventArgs> WindowPinned;


		public static void Pin(this IntPtr hWnd)
		{
			VirtualDesktop.PinWindow(hWnd);
			RaisePinnedEvent(hWnd, PinOperations.PinWindow);
		}

		public static void Unpin(this IntPtr hWnd)
		{
			VirtualDesktop.UnpinWindow(hWnd);
			RaisePinnedEvent(hWnd, PinOperations.UnpinWindow);
		}

		public static void TogglePin(this IntPtr hWnd)
		{
			if (VirtualDesktop.IsPinnedWindow(hWnd))
			{
				VirtualDesktop.UnpinWindow(hWnd);
				RaisePinnedEvent(hWnd, PinOperations.UnpinWindow);
			}
			else
			{
				VirtualDesktop.PinWindow(hWnd);
				RaisePinnedEvent(hWnd, PinOperations.PinWindow);
			}
		}

		public static void PinApp(this IntPtr hWnd)
		{
			var appId = ApplicationHelper.GetAppId(hWnd);
			if (appId == null) return;

			VirtualDesktop.PinApplication(appId);
			RaisePinnedEvent(hWnd, PinOperations.PinApp);
		}

		public static void UnpinApp(this IntPtr hWnd)
		{
			var appId = ApplicationHelper.GetAppId(hWnd);
			if (appId == null) return;

			VirtualDesktop.UnpinApplication(appId);
			RaisePinnedEvent(hWnd, PinOperations.UnpinApp);
		}

		public static void TogglePinApp(this IntPtr hWnd)
		{
			var appId = ApplicationHelper.GetAppId(hWnd);
			if (appId == null) return;

			if (VirtualDesktop.IsPinnedApplication(appId))
			{
				VirtualDesktop.UnpinApplication(appId);
				RaisePinnedEvent(hWnd, PinOperations.UnpinApp);
			}
			else
			{
				VirtualDesktop.PinApplication(appId);
				RaisePinnedEvent(hWnd, PinOperations.PinApp);
			}
		}

		private static void RaisePinnedEvent(IntPtr target, PinOperations operation)
		{
			WindowPinned?.Invoke(typeof(VirtualDesktopService), new WindowPinnedEventArgs(target, operation));
		}

		#endregion
	}

	internal class WindowPinnedEventArgs : EventArgs
	{
		public IntPtr Target { get; }
		public PinOperations PinOperation { get; }

		public WindowPinnedEventArgs(IntPtr target, PinOperations operation)
		{
			this.Target = target;
			this.PinOperation = operation;
		}
	}

	[Flags]
	internal enum PinOperations
	{
		Pin = 0x01,
		Unpin = 0x02,

		Window = 0x04,
		App = 0x08,

		PinWindow = Pin | Window,
		UnpinWindow = Unpin | Window,
		PinApp = Pin | App,
		UnpinApp = Unpin | App,
	}
}
