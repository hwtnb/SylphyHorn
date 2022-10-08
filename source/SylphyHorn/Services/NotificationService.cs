using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MetroTrilithon.Lifetime;
using SylphyHorn.Properties;
using SylphyHorn.Serialization;
using SylphyHorn.UI;
using SylphyHorn.UI.Bindings;
using WindowsDesktop;

namespace SylphyHorn.Services
{
	public class NotificationService : IDisposable
	{
		public static NotificationService Instance { get; } = new NotificationService();

		private static string ResidentHeader => Settings.General.SimpleNotification ? "" : "Virtual Desktop";

		private static string SwitchedHeader => Settings.General.SimpleNotification ? "" : "Virtual Desktop Switched";

		private static List<SwitchWindow> _residentWindows = new List<SwitchWindow>();

		private readonly SerialDisposable _notificationWindow = new SerialDisposable();

		public void ShowCurrentDesktop()
		{
			var current = VirtualDesktop.Current;
			var currentNumber = current.Index + 1;

			this._notificationWindow.Disposable = ShowDesktopWindow(currentNumber, ResidentHeader);
		}

		public void HideCurrentDesktop()
		{
			CloseResidentWindows();
		}

		public void ToggleCurrentDesktop()
		{
			if (_residentWindows.Count(window => window.IsVisible) == 0)
			{
				this.ShowCurrentDesktop();
			}
			else
			{
				this.HideCurrentDesktop();
			}
		}

		private NotificationService()
		{
			VirtualDesktop.CurrentChanged += this.VirtualDesktopOnCurrentChanged;
			VirtualDesktopService.WindowPinned += this.VirtualDesktopServiceOnWindowPinned;

			if (ProductInfo.IsReorderingSupportBuild) VirtualDesktop.Moved += this.VirtualDesktopOnMoved;
		}

		private void VirtualDesktopOnCurrentChanged(object sender, VirtualDesktopChangedEventArgs e)
		{
			if (!Settings.General.NotificationWhenSwitchedDesktop)
			{
				if (Settings.General.AlwaysShowDesktopNotification)
				{
					VisualHelper.InvokeOnUIDispatcher(() => Instance.ShowCurrentDesktop());
				}
				return;
			}

			VisualHelper.InvokeOnUIDispatcher(() =>
			{
				var desktops = VirtualDesktop.GetDesktops();
				var newIndex = Array.IndexOf(desktops, e.NewDesktop) + 1;

				this._notificationWindow.Disposable = ShowSwitchedDesktopWindow(newIndex);
			});
		}

		private void VirtualDesktopOnMoved(object sender, VirtualDesktopMovedEventArgs e)
		{
			if (!Settings.General.NotificationWhenSwitchedDesktop)
			{
				if (Settings.General.AlwaysShowDesktopNotification)
				{
					VisualHelper.InvokeOnUIDispatcher(() => Instance.ShowCurrentDesktop());
				}
				return;
			}

			VisualHelper.InvokeOnUIDispatcher(() =>
			{
				var current = VirtualDesktop.Current;
				var currentNumber = current.Index + 1;
				var newNumber = e.NewIndex + 1;
				var oldNumber = e.OldIndex + 1;

				this._notificationWindow.Disposable = ShowMovedDesktopWindow(currentNumber, newNumber, oldNumber);
			});
		}

		private void VirtualDesktopServiceOnWindowPinned(object sender, WindowPinnedEventArgs e)
		{
			VisualHelper.InvokeOnUIDispatcher(() =>
			{
				this._notificationWindow.Disposable = ShowPinWindow(e.Target, e.PinOperation);
			});
		}

		private static IDisposable ShowSwitchedDesktopWindow(int newNumber)
		{
			return ShowDesktopWindow(newNumber, SwitchedHeader);
		}

		private static IDisposable ShowMovedDesktopWindow(int currentNumber, int newNumber, int oldNumber)
		{
			return ShowDesktopWindow(
				header: Settings.General.SimpleNotification ? $"Desktop {oldNumber} => Desktop {newNumber}" : $"Desktop {oldNumber} Moved to Desktop {newNumber}",
				body: CreateNotificationBodyText(currentNumber));

			string CreateNotificationBodyText(int number)
			{
				var generalSttings = Settings.General;
				var desktopNames = generalSttings.DesktopNames.Value;
				var i = number - 1;
				if (!generalSttings.UseDesktopName || desktopNames.Count < number ||
					desktopNames[i].Value == null || desktopNames[i].Value.Length == 0)
				{
					var prefix = generalSttings.SimpleNotification ? "" : "Reordered Current Desktop: ";
					return prefix + "Desktop " + number.ToString();
				}
				else
				{
					return generalSttings.SimpleNotification
						? $"{number}. {desktopNames[i].Value}"
						: $"Reordered Desktop {number}: {desktopNames[i].Value}";
				}
			};
		}

		private static IDisposable ShowDesktopWindow(int newNumber, string header)
		{
			return ShowDesktopWindow(
				header: header,
				body: CreateNotificationBodyText(newNumber));

			string CreateNotificationBodyText(int number)
			{
				var generalSttings = Settings.General;
				var desktopNames = generalSttings.DesktopNames.Value;
				var i = number - 1;
				if (!generalSttings.UseDesktopName || desktopNames.Count < number ||
					desktopNames[i].Value == null || desktopNames[i].Value.Length == 0)
				{
					var prefix = generalSttings.SimpleNotification ? "" : "Current Desktop: ";
					return prefix + "Desktop " + number.ToString();
				}
				else
				{
					return generalSttings.SimpleNotification
						? $"{number}. {desktopNames[i].Value}"
						: $"Desktop {number}: {desktopNames[i].Value}";
				}
			};
		}

		private static IDisposable ShowDesktopWindow(string header, string body)
		{
			CloseResidentWindows();

			var source = new CancellationTokenSource();

			if (Settings.General.AlwaysShowDesktopNotification)
			{
				_residentWindows = CreateSwitchWindows(header, body);

				Task.Delay(TimeSpan.FromMilliseconds(Settings.General.NotificationDuration), source.Token)
					.ContinueWith(_ => _residentWindows.ForEach(window =>
					{
						window.DataContext = new NotificationWindowViewModel
						{
							Title = ProductInfo.Title,
							Header = ResidentHeader,
							Body = body,
						};
					}), source.Token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.FromCurrentSynchronizationContext());

				return Disposable.Create(() => source.Cancel());
			}
			else
			{
				var windows = CreateSwitchWindows(header, body);

				Task.Delay(TimeSpan.FromMilliseconds(Settings.General.NotificationDuration), source.Token)
					.ContinueWith(_ => windows.ForEach(window => window.Close()), TaskScheduler.FromCurrentSynchronizationContext());

				return Disposable.Create(() => source.Cancel());
			}
		}

		private static List<SwitchWindow> CreateSwitchWindows(string header, string body)
		{
			var vmodel = new NotificationWindowViewModel
			{
				Title = ProductInfo.Title,
				Header = header,
				Body = body,
			};

			var settings = Settings.General.Display.Value;
			Monitor[] targets;
			if (settings == 0)
			{
				targets = new[] { MonitorService.GetCurrentArea() };
			}
			else
			{
				var monitors = MonitorService.GetAreas();
				if (settings == uint.MaxValue)
				{
					targets = monitors;
				}
				else
				{
					targets = new[] { monitors[settings - 1] };
				}
			}

			return targets.Select(area =>
			{
				var window = new SwitchWindow(area.WorkArea)
				{
					DataContext = vmodel,
				};
				window.Show();
				return window;
			}).ToList();
		}

		private static void CloseResidentWindows()
		{
			if (_residentWindows.Count > 0)
			{
				_residentWindows.ForEach(window => window.Close());
				_residentWindows.Clear();
			}
		}

		private static IDisposable ShowPinWindow(IntPtr hWnd, PinOperations operation)
		{
			var vmodel = Settings.General.SimpleNotification
				? new NotificationWindowViewModel
				{
					Title = ProductInfo.Title,
					Header = "",
					Body = $"{(operation.HasFlag(PinOperations.Window) ? "Window" : "Application")} {(operation.HasFlag(PinOperations.Pin) ? "Pinned" : "Unpinned")}",
				}
				: new NotificationWindowViewModel
				{
					Title = ProductInfo.Title,
					Header = "Virtual Desktop",
					Body = $"{(operation.HasFlag(PinOperations.Pin) ? "Pinned" : "Unpinned")} this {(operation.HasFlag(PinOperations.Window) ? "window" : "application")}",
				};
			var source = new CancellationTokenSource();
			var window = new PinWindow(hWnd)
			{
				DataContext = vmodel,
			};
			window.Show();

			Task.Delay(TimeSpan.FromMilliseconds(Settings.General.NotificationDuration), source.Token)
				.ContinueWith(_ => window.Close(), TaskScheduler.FromCurrentSynchronizationContext());

			return Disposable.Create(() => source.Cancel());
		}

		public void Dispose()
		{
			VirtualDesktop.CurrentChanged -= this.VirtualDesktopOnCurrentChanged;
			VirtualDesktop.Moved -= this.VirtualDesktopOnMoved;
			VirtualDesktopService.WindowPinned -= this.VirtualDesktopServiceOnWindowPinned;

			this._notificationWindow.Dispose();
		}
	}
}
