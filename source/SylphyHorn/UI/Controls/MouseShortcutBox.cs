using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SylphyHorn.Serialization;
using SylphyHorn.Services;
using VirtualKey = System.Windows.Forms.Keys;

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

		#region Current 依存関係プロパティ

		public int[] Current
		{
			get { return (int[])this.GetValue(CurrentProperty); }
			set { this.SetValue(CurrentProperty, value); }
		}

		public static readonly DependencyProperty CurrentProperty =
			DependencyProperty.Register(nameof(Current), typeof(int[]), typeof(MouseShortcutBox), new UIPropertyMetadata(null, CurrentPropertyChangedCallback));

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

		protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			base.OnGotKeyboardFocus(e);

			this.Current = new int[0];
			this._pressedButton = VirtualKey.None;
			this._pressedSubs.Clear();
			this.UpdateText();
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				AddKeyCode(VirtualKey.LButton);
			}
			else
			{
				RemoveKeyCode(VirtualKey.LButton);
			}
			if (e.RightButton == MouseButtonState.Pressed)
			{
				AddKeyCode(VirtualKey.RButton);
			}
			else
			{
				RemoveKeyCode(VirtualKey.RButton);
			}
			if (e.MiddleButton == MouseButtonState.Pressed)
			{
				AddKeyCode(VirtualKey.MButton);
			}
			else
			{
				RemoveKeyCode(VirtualKey.MButton);
			}
			if (e.XButton1 == MouseButtonState.Pressed)
			{
				AddKeyCode(VirtualKey.XButton1);
			}
			else
			{
				RemoveKeyCode(VirtualKey.XButton1);
			}
			if (e.XButton2 == MouseButtonState.Pressed)
			{
				AddKeyCode(VirtualKey.XButton2);
			}
			else
			{
				RemoveKeyCode(VirtualKey.XButton2);
			}

			this.CurrentAsKeys = ValidateKeyCode()
				? this.GetShortcutKey()
				: (ShortcutKey?)null;

			this.UpdateText();

			e.Handled = true;
			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (!ValidateKeyCode())
			{
				this.UpdateText();
			}

			e.Handled = true;
			base.OnMouseUp(e);
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
				if (button == VirtualKey.LButton || button == VirtualKey.RButton)
				{
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
