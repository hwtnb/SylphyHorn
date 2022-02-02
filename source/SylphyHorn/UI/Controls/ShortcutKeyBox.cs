using System;
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
	[TemplatePart(Name = PART_ModifyKeys, Type = typeof(ItemsControl))]
	[TemplatePart(Name = PART_Key, Type = typeof(Keytop))]
	public class ShortcutKeyBox : TextBox
	{
		private const string PART_ModifyKeys = nameof(PART_ModifyKeys);
		private const string PART_Key = nameof(PART_Key);

		static ShortcutKeyBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(ShortcutKeyBox),
				new FrameworkPropertyMetadata(typeof(ShortcutKeyBox)));
		}

		private readonly HashSet<Key> _pressedModifiers = new HashSet<Key>();
		private Key _pressedKey = Key.None;
		private ItemsControl _modifiersPresneter;
		private Keytop _keyPresenter;
		private IDisposable _hookDisposable;

		public static HookService HookService { get; set; }

		#region Current 依存関係プロパティ

		public IList<int> Current
		{
			get { return (IList<int>)this.GetValue(CurrentProperty); }
			set { this.SetValue(CurrentProperty, value); }
		}

		public static readonly DependencyProperty CurrentProperty =
			DependencyProperty.Register(nameof(Current), typeof(IList<int>), typeof(ShortcutKeyBox), new UIPropertyMetadata(null, CurrentPropertyChangedCallback));

		private static void CurrentPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			var instance = (ShortcutKeyBox)d;
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

			this._modifiersPresneter = this.GetTemplateChild(PART_ModifyKeys) as ItemsControl;
			this._keyPresenter = this.GetTemplateChild(PART_Key) as Keytop;

			this.UpdateText();
		}

		protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			base.OnGotKeyboardFocus(e);

			this._hookDisposable = HookService?.Suspend();
			this.UpdateText();
		}

		protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			base.OnGotKeyboardFocus(e);

			this._hookDisposable?.Dispose();
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			if (!e.IsRepeat)
			{
				var key = e.Key == Key.System ? e.SystemKey : e.Key;
				if (key == Key.Back)
				{
					this._pressedModifiers.Clear();
					this._pressedKey = Key.None;
				}
				else if (key.IsModifyKey())
				{
					this._pressedModifiers.Add(key);
				}
				else
				{
					this._pressedKey = key;
				}

				this.CurrentAsKeys = /*this._pressedModifiers.Any() && */this._pressedKey != Key.None
					? this.GetShortcutKey()
					: (ShortcutKey?)null;

				this.UpdateText();
			}

			e.Handled = true;
			base.OnPreviewKeyDown(e);
		}

		protected override void OnPreviewKeyUp(KeyEventArgs e)
		{
			if (!e.IsRepeat)
			{
				var key = e.Key == Key.System ? e.SystemKey : e.Key;
				if (key.IsModifyKey())
				{
					this._pressedModifiers.Remove(key);
				}

				this._pressedKey = Key.None;
				this.UpdateText();
			}

			e.Handled = true;
			base.OnPreviewKeyUp(e);
		}

		private void UpdateText()
		{
			var currentKey = this.CurrentAsKeys ?? this.GetShortcutKey();

			if (this._modifiersPresneter != null)
			{
				var modifiers = (currentKey.ModifiersInternal ?? currentKey.Modifiers ?? Enumerable.Empty<VirtualKey>())
					.OrderBy(x => x)
					.ToArray();

				this._modifiersPresneter.ItemsSource = modifiers;
				this._modifiersPresneter.Visibility = modifiers.Length == 0
					? Visibility.Collapsed
					: Visibility.Visible;
			}

			if (this._keyPresenter != null)
			{
				this._keyPresenter.Key = currentKey.Key;
				this._keyPresenter.Visibility = currentKey.Key == VirtualKey.None
					? Visibility.Collapsed
					: Visibility.Visible;
			}
		}

		private ShortcutKey GetShortcutKey()
		{
			return new ShortcutKey(
				this._pressedKey.ToVirtualKey(),
				this._pressedModifiers.Select(x => x.ToVirtualKey()).ToArray());
		}
	}
}
