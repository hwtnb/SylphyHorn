using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SylphyHorn.Interop;
using NativeMethods = SylphyHorn.Interop.NativeMethods.GlobalHook;

namespace SylphyHorn.Services.Mouse
{
	/// <summary>
	/// Provides the function to intercept mouse events. (modified: https://qiita.com/exliko/items/3135e4413a6da067b35d)
	/// </summary>
	public class MouseInterceptor : IMouseInterceptor, IDisposable
	{
		//public event HookHandler MouseMove;
		public event HookHandler MouseDown;
		public event HookHandler MouseUp;
		public event HookHandler WheelDown;
		public event HookHandler WheelUp;

		public bool IsHooking
		{
			get;
			private set;
		}

		public bool IsSuspended
		{
			get;
			private set;
		}

		public bool SuppressEvent
		{
			get { return this._state.Handled; }
			private set { this._state.Handled = value; }
		}

		private readonly List<HookHandler> _lowLevelEvents = new List<HookHandler>();
		private event HookHandler _lowLevelHookEvent;
		private event NativeMethods.HookDelegate _nativeMethodCallback;

		private MouseState _state;
		private IntPtr _handle;

		public MouseInterceptor()
		{
			AddEvent(this.CaptureOnMouseEvent);
		}

		public void StartCapturing()
		{
			if (IsHooking)
			{
				return;
			}

			IsHooking = true;
			IsSuspended = false;

			this._nativeMethodCallback = HookProcedure;
			IntPtr h = Marshal.GetHINSTANCE(typeof(MouseInterceptor).Assembly.GetModules()[0]);

			// WH_MOUSE_LL = 14
			this._handle = NativeMethods.SetWindowsHookEx(14, this._nativeMethodCallback, h, 0);

			if (this._handle == IntPtr.Zero)
			{
				IsHooking = false;
				IsSuspended = true;

				throw new System.ComponentModel.Win32Exception();
			}
		}

		public void StopCapturing()
		{
			if (!IsHooking)
			{
				return;
			}

			if (this._handle != IntPtr.Zero)
			{
				IsHooking = false;
				IsSuspended = true;

				ClearEvent();

				NativeMethods.UnhookWindowsHookEx(_handle);
				this._handle = IntPtr.Zero;
				this._nativeMethodCallback -= HookProcedure;
			}
		}

		public void SuspendCapturing()
		{
			IsSuspended = true;
		}

		private void CaptureOnMouseEvent(ref MouseState state)
		{
			if (!IsHooking) return;

			// Skip unused events
			/*if (state.Stroke == Stroke.Move)
			{
				CaptureOnMouseMove(ref state);
			}
			else */
			if (state.Stroke == Stroke.WheelDown)
			{
				CaptureOnMouseWheelDown(ref state);
			}
			else if (state.Stroke == Stroke.WheelUp)
			{
				CaptureOnMouseWheelUp(ref state);
			}
			else if (state.Direction == StrokeDirection.Down)
			{
				CaptureOnMouseDown(ref state);
			}
			else if (state.Direction == StrokeDirection.Up)
			{
				CaptureOnMouseUp(ref state);
			}
		}

		/*
		private void CaptureOnMouseMove(ref MouseState state)
		{
			this.MouseMove?.Invoke(ref state);
		}
		*/

		private void CaptureOnMouseWheelDown(ref MouseState state)
		{
			this.WheelDown?.Invoke(ref state);
		}

		private void CaptureOnMouseWheelUp(ref MouseState state)
		{
			this.WheelUp?.Invoke(ref state);
		}

		private void CaptureOnMouseDown(ref MouseState state)
		{
			var keyCode = state.KeyCode;
			if (keyCode < Keys.LButton || keyCode > Keys.XButton2 || keyCode == Keys.Cancel) return;

			this.MouseDown?.Invoke(ref state);
		}

		private void CaptureOnMouseUp(ref MouseState state)
		{
			var keyCode = state.KeyCode;
			if (keyCode < Keys.LButton || keyCode > Keys.XButton2 || keyCode == Keys.Cancel) return;

			this.MouseUp?.Invoke(ref state);
		}

		private void AddEvent(HookHandler hookHandler)
		{
			this._lowLevelEvents.Add(hookHandler);
			this._lowLevelHookEvent += hookHandler;
		}

		private void RemoveEvent(HookHandler hookHandler)
		{
			if (this._lowLevelEvents.Count == 0)
			{
				return;
			}

			this._lowLevelHookEvent -= hookHandler;
			this._lowLevelEvents.Remove(hookHandler);
		}

		private void ClearEvent()
		{
			if (this._lowLevelEvents.Count == 0)
			{
				return;
			}

			foreach (HookHandler e in this._lowLevelEvents)
			{
				this._lowLevelHookEvent -= e;
			}

			this._lowLevelEvents.Clear();
		}

		private IntPtr HookProcedure(int nCode, uint msg, ref MSLLHOOKSTRUCT s)
		{
			if (nCode >= 0 && this._lowLevelHookEvent != null && !IsSuspended)
			{
				var stroke = GetStroke(msg, ref s);
				// Skip unused events
				if (stroke == Stroke.Move || stroke == Stroke.Unknown)
				{
					return NativeMethods.CallNextHookEx(this._handle, nCode, msg, ref s);
				}
				this._state.Stroke = stroke;
				this._state.X = s.pt.X;
				this._state.Y = s.pt.Y;
				this._state.Data = s.mouseData;
				this._state.Flags = s.flags;
				this._state.Time = s.time;
				this._state.ExtraInfo = s.dwExtraInfo;
				this._state.Handled = false;

				// Skip unused events
				/*if (stroke == Stroke.Move || stroke == Stroke.Unknown)
				{
					this._state.KeyCode = Keys.None;
					this._state.Direction = StrokeDirection.None;
				}
				else */
				if (stroke == Stroke.WheelDown || stroke == Stroke.WheelUp)
				{
					this._state.KeyCode = (Keys)stroke;
					this._state.Direction = StrokeDirection.None;
				}
				else if((int)stroke % 2 != 0)
				{
					this._state.KeyCode = (Keys)(((int)stroke >> 1) + 1);
					this._state.Direction = StrokeDirection.Down;
				}
				else
				{
					this._state.KeyCode = (Keys)((int)stroke >> 1);
					this._state.Direction = StrokeDirection.Up;
				}

				this._lowLevelHookEvent(ref this._state);

				if (SuppressEvent)
				{
					SuppressEvent = false;

					return (IntPtr)1;
				}
			}

			return NativeMethods.CallNextHookEx(this._handle, nCode, msg, ref s);
		}

		private Stroke GetStroke(uint msg, ref MSLLHOOKSTRUCT s)
		{
			switch (msg)
			{
				case 0x0200:
					// WM_MOUSEMOVE
					return Stroke.Move;
				case 0x0201:
					// WM_LBUTTONDOWN
					return Stroke.LeftDown;
				case 0x0202:
					// WM_LBUTTONUP
					return Stroke.LeftUp;
				case 0x0204:
					// WM_RBUTTONDOWN
					return Stroke.RightDown;
				case 0x0205:
					// WM_RBUTTONUP
					return Stroke.RightUp;
				case 0x0207:
					// WM_MBUTTONDOWN
					return Stroke.MiddleDown;
				case 0x0208:
					// WM_MBUTTONUP
					return Stroke.MiddleUp;
				case 0x020A:
					// WM_MOUSEWHEE
					return ((short)((s.mouseData >> 16) & 0xffff) > 0) ? Stroke.WheelUp : Stroke.WheelDown;
				case 0x20B:
					// WM_XBUTTONDOWN
					switch (s.mouseData >> 16)
					{
						case 1:
							return Stroke.X1Down;
						case 2:
							return Stroke.X2Down;
						default:
							return Stroke.Unknown;
					}
				case 0x20C:
					// WM_XBUTTONUP
					switch (s.mouseData >> 16)
					{
						case 1:
							return Stroke.X1Up;
						case 2:
							return Stroke.X2Up;
						default:
							return Stroke.Unknown;
					}
				default:
					return Stroke.Unknown;
			}
		}

		public void Dispose()
		{
			StopCapturing();
		}

		~MouseInterceptor()
		{
			this.Dispose();
		}
	}
}
