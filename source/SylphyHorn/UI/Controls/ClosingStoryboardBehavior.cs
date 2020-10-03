using System;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;

namespace SylphyHorn.UI.Controls
{
	public class ClosingStoryboardBehavior : Behavior<Window>
	{
		private bool _canClose;

		#region Storyboard 依存関係プロパティ

		public Storyboard Storyboard
		{
			get { return (Storyboard)this.GetValue(StoryboardProperty); }
			set { this.SetValue(StoryboardProperty, value); }
		}
		public static readonly DependencyProperty StoryboardProperty =
			DependencyProperty.Register(nameof(Storyboard), typeof(Storyboard), typeof(ClosingStoryboardBehavior), new UIPropertyMetadata(null, StoryboardChangedCallback));

		private static void StoryboardChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			var instance = (ClosingStoryboardBehavior)d;
			var oldValue = (Storyboard)args.OldValue;
			var newValue = (Storyboard)args.NewValue;

			if (oldValue != null)
			{
				oldValue.Completed -= instance.HandleStorybarodCompleted;
			}
			if (newValue != null)
			{
				newValue.Completed += instance.HandleStorybarodCompleted; 
			}
		}

		#endregion

		protected override void OnAttached()
		{
			base.OnAttached();
			this.AssociatedObject.Closing += (sender, args) =>
			{
				if (this.Storyboard == null || args.Cancel || this._canClose) return;

				args.Cancel = true;
				this.Storyboard.Begin();
			};
		}

		private void HandleStorybarodCompleted(object sender, EventArgs args)
		{
			this._canClose = true;
			this.AssociatedObject.Close();
		}
	}
}
