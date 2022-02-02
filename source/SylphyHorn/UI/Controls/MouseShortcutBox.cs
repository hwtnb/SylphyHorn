using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SylphyHorn.Serialization;
using SylphyHorn.Services;
using VirtualKey = System.Windows.Forms.Keys;
using MouseStroke = SylphyHorn.Services.Mouse.Stroke;

namespace SylphyHorn.UI.Controls
{
	[TemplatePart(Name = PART_SubButtons, Type = typeof(ItemsControl))]
	[TemplatePart(Name = PART_MainButton, Type = typeof(Keytop))]
	public class MouseShortcutBox : TextBox
	{
		private const string PART_SubButtons = nameof(PART_SubButtons);
		private const string PART_MainButton = nameof(PART_MainButton);

		static MouseShortcutBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(MouseShortcutBox),
				new FrameworkPropertyMetadata(typeof(MouseShortcutBox)));
		}

		private readonly List<VirtualKey> _pressedSubs = new List<VirtualKey>(5);
		private VirtualKey _pressedButton = VirtualKey.None;
		private ItemsControl _subPresneter;
		private Keytop _mainPresenter;
		private bool _focus;
		private IDisposable _hookDisposable;

		public static HookService HookService { get; set; }

		#region Current 依存関係プロパティ

		public IList<int> Current
		{
			get { return (IList<int>)this.GetValue(CurrentProperty); }
			set { this.SetValue(CurrentProperty, value); }
		}

		public static readonly DependencyProperty CurrentProperty =
			DependencyProperty.Register(nameof(Current), typeof(IList<int>), typeof(MouseShortcutBox), new UIPropertyMetadata(null, CurrentPropertyChangedCallback));

		private static void CurrentPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			var instance = (MouseShortcutBox)d;
			instance.UpdateText();
		}

		private ShortcutKey? CurrentAsKeys
		{
			get { return this.Current?.ToShortcutKey(); }
			set { this.Current = value?.ToSerializable(); }
		}

		#endregion

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this._subPresneter = this.GetTemplateChild(PART_SubButtons) as ItemsControl;
			this._mainPresenter = this.GetTemplateChild(PART_MainButton) as Keytop;

			this.UpdateText();
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			if (!this._focus)
			{
				this.CurrentAsKeys = null;
				this._pressedButton = VirtualKey.None;
				this._pressedSubs.Clear();
				this._hookDisposable = HookService?.Suspend();
				this._focus = true;
			}

			this.UpdateKeyCode(e);

			this.CurrentAsKeys = this._pressedButton != VirtualKey.None
				? this.GetShortcutKey()
				: (ShortcutKey?)null;

			this.UpdateText();

			e.Handled = true;
			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (!this.ValidateKeyCode())
			{
				this.CurrentAsKeys = null;
				this.UpdateText();
			}

			this._hookDisposable?.Dispose();
			this._focus = false;

			e.Handled = true;
			base.OnMouseUp(e);
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			this.UpdateKeyCode(e);

			var current = this._pressedButton;
			if (this._pressedSubs.Count > 0 ||
				(VirtualKey.LButton <= current && current <= VirtualKey.XButton2 && current != VirtualKey.Cancel))
			{
				if (e.Delta < 0)
				{
					this.AddKeyCode((VirtualKey)MouseStroke.WheelDown);
				}
				else
				{
					this.AddKeyCode((VirtualKey)MouseStroke.WheelUp);
				}

				this.CurrentAsKeys = this.ValidateKeyCode()
					? this.GetShortcutKey()
					: (ShortcutKey?)null;

				this.UpdateText();
			}

			e.Handled = true;
			base.OnMouseWheel(e);
		}

		private void UpdateText()
		{
			var currentKey = this.CurrentAsKeys ?? this.GetShortcutKey();

			if (this._subPresneter != null)
			{
				var buttons = (currentKey.ModifiersInternal ?? currentKey.Modifiers ?? Enumerable.Empty<VirtualKey>())
					.OrderBy(x => x)
					.ToArray();

				this._subPresneter.ItemsSource = buttons;
				this._subPresneter.Visibility = buttons.Length == 0
					? Visibility.Collapsed
					: Visibility.Visible;
			}

			if (this._mainPresenter != null)
			{
				this._mainPresenter.Key = currentKey.Key;
				this._mainPresenter.Visibility = currentKey.Key == VirtualKey.None
					? Visibility.Collapsed
					: Visibility.Visible;
			}
		}

		private void UpdateKeyCode(MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				this.AddKeyCode(VirtualKey.LButton);
			}
			else
			{
				this.RemoveKeyCode(VirtualKey.LButton);
			}
			if (e.RightButton == MouseButtonState.Pressed)
			{
				this.AddKeyCode(VirtualKey.RButton);
			}
			else
			{
				this.RemoveKeyCode(VirtualKey.RButton);
			}
			if (e.MiddleButton == MouseButtonState.Pressed)
			{
				this.AddKeyCode(VirtualKey.MButton);
			}
			else
			{
				this.RemoveKeyCode(VirtualKey.MButton);
			}
			if (e.XButton1 == MouseButtonState.Pressed)
			{
				this.AddKeyCode(VirtualKey.XButton1);
			}
			else
			{
				this.RemoveKeyCode(VirtualKey.XButton1);
			}
			if (e.XButton2 == MouseButtonState.Pressed)
			{
				this.AddKeyCode(VirtualKey.XButton2);
			}
			else
			{
				this.RemoveKeyCode(VirtualKey.XButton2);
			}
		}

		private void AddKeyCode(VirtualKey keyCode)
		{
			var currentKey = this._pressedButton;
			if (currentKey == VirtualKey.None)
			{
				this._pressedButton = keyCode;
			}
			else if (currentKey != keyCode && !this._pressedSubs.Contains(keyCode))
			{
				this._pressedButton = keyCode;
				this._pressedSubs.Add(currentKey);
			}
		}

		private void RemoveKeyCode(VirtualKey keyCode)
		{
			var currentKey = this._pressedButton;
			if (currentKey == keyCode)
			{
				keyCode = this._pressedSubs.FirstOrDefault();
				this._pressedButton = keyCode;
			}
			this._pressedSubs.Remove(keyCode);
		}

		private bool ValidateKeyCode()
		{
			var count = this._pressedSubs.Count;
			if (count == 0)
			{
				var button = this._pressedButton;
				if (button == VirtualKey.LButton || button == VirtualKey.RButton ||
					button == (VirtualKey)MouseStroke.WheelDown || button == (VirtualKey)MouseStroke.WheelUp)
				{
					this._pressedButton = VirtualKey.None;
					return false;
				}
			}
			else
			{
				var firstSub = this._pressedSubs[0];
				if (firstSub == (VirtualKey)MouseStroke.WheelDown || firstSub == (VirtualKey)MouseStroke.WheelUp)
				{
					this._pressedSubs.Clear();
					this._pressedButton = VirtualKey.None;
					return false;
				}
			}
			return true;
		}

		private ShortcutKey GetShortcutKey()
		{
			return new ShortcutKey(
				this._pressedButton,
				this._pressedSubs.ToArray());
		}
	}
}
