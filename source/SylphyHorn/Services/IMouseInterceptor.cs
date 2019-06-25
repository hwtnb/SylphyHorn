using System;
using System.Windows.Forms;

namespace SylphyHorn.Services.Mouse
{
	/// <summary>
	/// Provides the interface to intercept mouse events.
	/// </summary>
	public interface IMouseInterceptor
	{
		/// <summary>
		/// Occurs when detects a mouse event.
		/// </summary>
		//event HookHandler MouseMove;
		event HookHandler MouseDown;
		event HookHandler MouseUp;
		event HookHandler WheelDown;
		event HookHandler WheelUp;

		bool IsHooking
		{
			get;
		}

		bool IsSuspended
		{
			get;
		}

		bool SuppressEvent
		{
			get;
		}

		void StartCapturing();
		void StopCapturing();
		void SuspendCapturing();
	}

	public struct MouseState
	{
		public Stroke Stroke;
		public Keys KeyCode;
		public StrokeDirection Direction;
		public int X;
		public int Y;
		public uint Data;
		public uint Flags;
		public uint Time;
		public IntPtr ExtraInfo;
		public bool Handled;
	}

	public enum Stroke
	{
		Move       = 0,
		LeftDown   = 1,
		LeftUp     = 2,
		RightDown  = 3,
		RightUp    = 4,
		WheelDown  = Keys.Alt * 2 + 1,
		WheelUp    = Keys.Alt * 2 + 2,
		MiddleDown = 7,
		MiddleUp   = 8,
		X1Down     = 9,
		X1Up       = 10,
		X2Down     = 11,
		X2Up       = 12,
		Unknown    = 13,
	}

	public enum StrokeDirection
	{
		None,
		Up,
		Down,
	}

	public delegate void HookHandler(ref MouseState state);
}
