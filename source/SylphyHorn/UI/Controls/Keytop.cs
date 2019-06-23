using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VirtualKey = System.Windows.Forms.Keys;

namespace SylphyHorn.UI.Controls
{
	public enum KeyKind
	{
		Normal,
		Left,
		Right,
	}

	public class Keytop : Control
	{
		static Keytop()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(Keytop),
				new FrameworkPropertyMetadata(typeof(Keytop)));
		}
		
		#region Key dependency property

		public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
			nameof(Key), typeof(VirtualKey), typeof(Keytop), new PropertyMetadata(default(VirtualKey), HandleKeyPropertyChanged));

		private static void HandleKeyPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			var instance = (Keytop)sender;
			var newValue = (VirtualKey)args.NewValue;

			var tuple = GetKeyString(newValue);
			instance.KeyText = tuple.Item1;
			instance.KeyKind = tuple.Item2;
		}

		public VirtualKey Key
		{
			get { return (VirtualKey)this.GetValue(KeyProperty); }
			set { this.SetValue(KeyProperty, value); }
		}

		#endregion

		#region KeyKind dependency property

		public static readonly DependencyProperty KeyKindProperty = DependencyProperty.Register(
			nameof(KeyKind), typeof(KeyKind), typeof(Keytop), new PropertyMetadata(default(KeyKind)));

		public KeyKind KeyKind
		{
			get { return (KeyKind)this.GetValue(KeyKindProperty); }
			private set { this.SetValue(KeyKindProperty, value); }
		}

		#endregion

		#region KeyText dependency property

		public static readonly DependencyProperty KeyTextProperty = DependencyProperty.Register(
			nameof(KeyText), typeof(string), typeof(Keytop), new PropertyMetadata(default(string)));

		public string KeyText
		{
			get { return (string)this.GetValue(KeyTextProperty); }
			private set { this.SetValue(KeyTextProperty, value); }
		}

		#endregion
		
		private static Tuple<string, KeyKind> GetKeyString(VirtualKey key)
		{
			string text;
			KeyKind kind;

			switch (key)
			{
				case VirtualKey.LShiftKey:
					text = "Shift";
					kind = KeyKind.Left;
					break;
				case VirtualKey.RShiftKey:
					text = "Shift";
					kind = KeyKind.Right;
					break;

				case VirtualKey.LMenu:
					text = "Alt";
					kind = KeyKind.Left;
					break;
				case VirtualKey.RMenu:
					text = "Alt";
					kind = KeyKind.Right;
					break;

				case VirtualKey.LControlKey:
					text = "Ctrl";
					kind = KeyKind.Left;
					break;
				case VirtualKey.RControlKey:
					text = "Ctrl";
					kind = KeyKind.Right;
					break;

				case VirtualKey.LWin:
					text = "Win";
					kind = KeyKind.Left;
					break;
				case VirtualKey.RWin:
					text = "Win";
					kind = KeyKind.Right;
					break;

				case VirtualKey.Control:
					text = "Ctrl";
					kind = KeyKind.Normal;
					break;
				
				case VirtualKey.NoName:
					// shoddy construction :(
					text = "Win";
					kind = KeyKind.Normal;
					break;

				case VirtualKey.LButton:
					// Mouse Button
					text = "Left Button";
					kind = KeyKind.Normal;
					break;

				case VirtualKey.RButton:
					// Mouse Button
					text = "Right Button";
					kind = KeyKind.Normal;
					break;

				case VirtualKey.MButton:
					// Mouse Button
					text = "Middle Button";
					kind = KeyKind.Normal;
					break;

				case VirtualKey.XButton1:
					// Mouse Button
					text = "XButton 1";
					kind = KeyKind.Normal;
					break;

				case VirtualKey.XButton2:
					// Mouse Button
					text = "XButton 2";
					kind = KeyKind.Normal;
					break;

				default:
					text = key.ToString();
					kind = KeyKind.Normal;
					break;
			}

			return Tuple.Create(text, kind);
		}
	}
}
