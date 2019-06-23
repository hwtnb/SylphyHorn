using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Open.WinKeyboardHook;
using SylphyHorn.Services.Mouse;

namespace SylphyHorn.Services
{
	/// <summary>
	/// Provides the function to detect a shortcut key ([modifier key(s)] + [key] style) by use of global key hook.
	/// </summary>
	public class ShortcutKeyDetector
	{
		private readonly HashSet<Keys> _pressedModifiers = new HashSet<Keys>();
		private readonly HashSet<Keys> _pressedMouseButtons = new HashSet<Keys>();
		private readonly IKeyboardInterceptor _keyInterceptor = new KeyboardInterceptor();
		private readonly IMouseInterceptor _mouseInterceptor = new MouseInterceptor();

		private bool _started;
		private bool _suspended;

		/// <summary>
		/// Occurs when detects a shortcut key.
		/// </summary>
		public event EventHandler<ShortcutKeyPressedEventArgs> Pressed;
		public event EventHandler<ShortcutKeyPressedEventArgs> Up;

		public ShortcutKeyDetector()
		{
			this._keyInterceptor.KeyDown += this.InterceptorOnKeyDown;
			this._keyInterceptor.KeyUp += this.InterceptorOnKeyUp;
			this._mouseInterceptor.MouseDown += this.InterceptorOnMouseDown;
			this._mouseInterceptor.MouseUp += this.InterceptorOnMouseUp;
		}

		public void Start()
		{
			if (!this._started)
			{
				this._keyInterceptor.StartCapturing();
				this._mouseInterceptor.StartCapturing();
				this._started = true;
			}

			this._suspended = false;
		}

		public void Stop()
		{
			this._suspended = true;
			this._pressedModifiers.Clear();
			this._pressedMouseButtons.Clear();
		}

		private void InterceptorOnKeyDown(object sender, KeyEventArgs args)
		{
			if (this._suspended) return;

			if (args.KeyCode.IsModifyKey())
			{
				this._pressedModifiers.Add(args.KeyCode);
			}
			else
			{
				var pressedEventArgs = new ShortcutKeyPressedEventArgs(args.KeyCode, this._pressedModifiers);
				this.Pressed?.Invoke(this, pressedEventArgs);
				if (pressedEventArgs.Handled) args.SuppressKeyPress = true;
			}
		}

		private void InterceptorOnKeyUp(object sender, KeyEventArgs args)
		{
			if (this._suspended) return;

			//if (this._pressedModifiers.Count == 0) return;

			if (args.KeyCode.IsModifyKey())
			{
				this._pressedModifiers.Remove(args.KeyCode);
			}
			else
			{
				var pressedEventArgs = new ShortcutKeyPressedEventArgs(args.KeyCode, this._pressedModifiers);
				this.Up?.Invoke(this, pressedEventArgs);
				if (pressedEventArgs.Handled) args.SuppressKeyPress = true;
			}
		}

		private void InterceptorOnMouseDown(ref MouseState state)
		{
			if (this._suspended) return;

			var keyCode = state.KeyCode;
			if (keyCode < Keys.LButton || keyCode > Keys.XButton2 || keyCode == Keys.Cancel) return;

			var pressedEventArgs = new ShortcutKeyPressedEventArgs(keyCode, this._pressedMouseButtons);
			this.Pressed?.Invoke(this, pressedEventArgs);
			state.Handled = pressedEventArgs.Handled;

			this._pressedMouseButtons.Add(keyCode);
		}

		private void InterceptorOnMouseUp(ref MouseState state)
		{
			if (this._suspended) return;

			var keyCode = state.KeyCode;
			if (keyCode < Keys.LButton || keyCode > Keys.XButton2 || keyCode == Keys.Cancel) return;

			this._pressedMouseButtons.Remove(keyCode);

			var pressedEventArgs = new ShortcutKeyPressedEventArgs(keyCode, this._pressedMouseButtons);
			this.Up?.Invoke(this, pressedEventArgs);
			state.Handled = pressedEventArgs.Handled;
		}
	}
}
