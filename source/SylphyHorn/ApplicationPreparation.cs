using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WindowsDesktop;
using MetroTrilithon.Lifetime;
using SylphyHorn.Interop;
using SylphyHorn.Properties;
using SylphyHorn.Serialization;
using SylphyHorn.Services;
using SylphyHorn.UI;
using SylphyHorn.UI.Bindings;

namespace SylphyHorn
{
	using ActionRegister1 = Func<Func<ShortcutKey>, Action<IntPtr>, IDisposable>;
	using ActionRegister2 = Func<Func<ShortcutKey>, Action<IntPtr>, Func<bool>, IDisposable>;

	public class ApplicationPreparation
	{
		private readonly HookService _hookService;
		private readonly Action _shutdownAction;
		private readonly IDisposableHolder _disposable;
		private TaskTrayIcon _taskTrayIcon;

		public event Action VirtualDesktopInitialized;

		public event Action VirtualDesktopInitializationCanceled;

		public event Action<Exception> VirtualDesktopInitializationFailed;

		public ApplicationPreparation(HookService hookService, Action shutdownAction, IDisposableHolder disposable)
		{
			this._hookService = hookService;
			this._shutdownAction = shutdownAction;
			this._disposable = disposable;

			this._hookService.Reload = () =>
			{
				this.RegisterActions();
			};
		}

		public void RegisterActions()
		{
			RegisterActions(Settings.ShortcutKey, this._hookService.RegisterKeyAction, this._hookService.RegisterKeyAction);
			RegisterActions(Settings.MouseShortcut, this._hookService.RegisterMouseAction, this._hookService.RegisterMouseAction);
		}

		private void RegisterActions(ShortcutKeySettings settings, ActionRegister1 register1, ActionRegister2 register2)
		{
			register1(() => settings.MoveLeft.ToShortcutKey(), hWnd => hWnd.MoveToLeft())
				.AddTo(this._disposable);

			register1(() => settings.MoveLeftAndSwitch.ToShortcutKey(), hWnd => hWnd.MoveToLeft()?.Switch())
				.AddTo(this._disposable);

			register1(() => settings.MoveRight.ToShortcutKey(), hWnd => hWnd.MoveToRight())
				.AddTo(this._disposable);

			register1(() => settings.MoveRightAndSwitch.ToShortcutKey(), hWnd => hWnd.MoveToRight()?.Switch())
				.AddTo(this._disposable);

			register1(() => settings.MoveNew.ToShortcutKey(), hWnd => hWnd.MoveToNew())
				.AddTo(this._disposable);

			register1(() => settings.MoveNewAndSwitch.ToShortcutKey(), hWnd => hWnd.MoveToNew()?.Switch())
				.AddTo(this._disposable);

			var isKeyboardSettings = settings as MouseShortcutSettings == null;
			if (isKeyboardSettings)
			{
				if (Settings.General.OverrideWindowsDefaultKeyCombination)
				{
					register1(() => settings.SwitchToLeftWithDefault.ToShortcutKey(), _ => { })
						.AddTo(this._disposable);

					register1(() => settings.SwitchToRightWithDefault.ToShortcutKey(), _ => { })
						.AddTo(this._disposable);
				}
				else
				{
					register2(
							() => settings.SwitchToLeftWithDefault.ToShortcutKey(),
							_ => VirtualDesktopService.GetLeft()?.Switch(),
							() => Settings.General.ChangeBackgroundEachDesktop)
						.AddTo(this._disposable);

					register2(
							() => settings.SwitchToRightWithDefault.ToShortcutKey(),
							_ => VirtualDesktopService.GetRight()?.Switch(),
							() => Settings.General.ChangeBackgroundEachDesktop)
						.AddTo(this._disposable);
				}

				register1(() => settings.SwitchToLeft.ToShortcutKey(), _ => VirtualDesktopService.GetLeft()?.Switch())
					.AddTo(this._disposable);

				register1(() => settings.SwitchToRight.ToShortcutKey(), _ => VirtualDesktopService.GetRight()?.Switch())
					.AddTo(this._disposable);
			}
			else
			{
				register1(() => settings.SwitchToLeft.ToShortcutKey(), _ => VirtualDesktopService.GetLeft()?.Switch())
					.AddTo(this._disposable);

				register1(() => settings.SwitchToRight.ToShortcutKey(), _ => VirtualDesktopService.GetRight()?.Switch())
					.AddTo(this._disposable);
			}

			register1(() => settings.CloseAndSwitchLeft.ToShortcutKey(), _ => VirtualDesktopService.CloseAndSwitchLeft())
				.AddTo(this._disposable);

			register1(() => settings.CloseAndSwitchRight.ToShortcutKey(), _ => VirtualDesktopService.CloseAndSwitchRight())
				.AddTo(this._disposable);

			register1(() => settings.ShowTaskView.ToShortcutKey(), _ => VirtualDesktopService.ShowTaskView())
				.AddTo(this._disposable);

			register1(() => settings.Pin.ToShortcutKey(), hWnd => hWnd.Pin())
				.AddTo(this._disposable);

			register1(() => settings.Unpin.ToShortcutKey(), hWnd => hWnd.Unpin())
				.AddTo(this._disposable);

			register1(() => settings.TogglePin.ToShortcutKey(), hWnd => hWnd.TogglePin())
				.AddTo(this._disposable);

			register1(() => settings.PinApp.ToShortcutKey(), hWnd => hWnd.PinApp())
				.AddTo(this._disposable);

			register1(() => settings.UnpinApp.ToShortcutKey(), hWnd => hWnd.UnpinApp())
				.AddTo(this._disposable);

			register1(() => settings.TogglePinApp.ToShortcutKey(), hWnd => hWnd.TogglePinApp())
				.AddTo(this._disposable);

			var keyIndex = 0;
			RegisterSpecifiedDesktopSwitching(keyIndex++, settings.SwitchToIndex0.ToShortcutKey());
			RegisterSpecifiedDesktopSwitching(keyIndex++, settings.SwitchToIndex1.ToShortcutKey());
			RegisterSpecifiedDesktopSwitching(keyIndex++, settings.SwitchToIndex2.ToShortcutKey());
			RegisterSpecifiedDesktopSwitching(keyIndex++, settings.SwitchToIndex3.ToShortcutKey());
			RegisterSpecifiedDesktopSwitching(keyIndex++, settings.SwitchToIndex4.ToShortcutKey());
			RegisterSpecifiedDesktopSwitching(keyIndex++, settings.SwitchToIndex5.ToShortcutKey());
			RegisterSpecifiedDesktopSwitching(keyIndex++, settings.SwitchToIndex6.ToShortcutKey());
			RegisterSpecifiedDesktopSwitching(keyIndex++, settings.SwitchToIndex7.ToShortcutKey());
			RegisterSpecifiedDesktopSwitching(keyIndex++, settings.SwitchToIndex8.ToShortcutKey());
			RegisterSpecifiedDesktopSwitching(keyIndex++, settings.SwitchToIndex9.ToShortcutKey());

			keyIndex = 0;
			RegisterMovingToSpecifiedDesktop(keyIndex++, settings.MoveToIndex0.ToShortcutKey());
			RegisterMovingToSpecifiedDesktop(keyIndex++, settings.MoveToIndex1.ToShortcutKey());
			RegisterMovingToSpecifiedDesktop(keyIndex++, settings.MoveToIndex2.ToShortcutKey());
			RegisterMovingToSpecifiedDesktop(keyIndex++, settings.MoveToIndex3.ToShortcutKey());
			RegisterMovingToSpecifiedDesktop(keyIndex++, settings.MoveToIndex4.ToShortcutKey());
			RegisterMovingToSpecifiedDesktop(keyIndex++, settings.MoveToIndex5.ToShortcutKey());
			RegisterMovingToSpecifiedDesktop(keyIndex++, settings.MoveToIndex6.ToShortcutKey());
			RegisterMovingToSpecifiedDesktop(keyIndex++, settings.MoveToIndex7.ToShortcutKey());
			RegisterMovingToSpecifiedDesktop(keyIndex++, settings.MoveToIndex8.ToShortcutKey());
			RegisterMovingToSpecifiedDesktop(keyIndex++, settings.MoveToIndex9.ToShortcutKey());

			void RegisterSpecifiedDesktopSwitching(int i, ShortcutKey shortcut)
			{
				register1(() => shortcut, _ => VirtualDesktopService.GetByIndex(i)?.Switch())
					.AddTo(this._disposable);
			};

			void RegisterMovingToSpecifiedDesktop(int i, ShortcutKey shortcut)
			{
				register1(() => shortcut, hWnd => hWnd.MoveToIndex(i))
					.AddTo(this._disposable);
			};
		}

		public TaskTrayIcon CreateTaskTrayIcon()
		{
			if (this._taskTrayIcon == null)
			{
				const string iconUri = "pack://application:,,,/SylphyHorn;Component/.assets/tasktray.ico";

				if (!Uri.TryCreate(iconUri, UriKind.Absolute, out var uri)) return null;

				var icon = IconHelper.GetIconFromResource(uri);
				var menus = new[]
				{
					new TaskTrayIconItem(Resources.TaskTray_Menu_Settings, ShowSettings, () => Application.Args.CanSettings),
					new TaskTrayIconItem(Resources.TaskTray_Menu_Exit, this._shutdownAction),
				};

				this._taskTrayIcon = new TaskTrayIcon(icon, menus);
			}

			return this._taskTrayIcon;

			void ShowSettings()
			{
				using (this._hookService.Suspend())
				{
					if (SettingsWindow.Instance != null)
					{
						SettingsWindow.Instance.Activate();
					}
					else
					{
						SettingsWindow.Instance = new SettingsWindow
						{
							DataContext = new SettingsWindowViewModel(this._hookService),
						};

						SettingsWindow.Instance.ShowDialog();
						SettingsWindow.Instance = null;
					}
				}
			}
		}

		public TaskTrayBaloon CreateFirstTimeBaloon()
		{
			var baloon = this.CreateTaskTrayIcon().CreateBaloon();
			baloon.Title = ProductInfo.Title;
			baloon.Text = Resources.TaskTray_FirstTimeMessage;
			baloon.Timespan = TimeSpan.FromMilliseconds(5000);

			return baloon;
		}

		public void PrepareVirtualDesktop()
		{
			var provider = new VirtualDesktopProvider()
			{
				ComInterfaceAssemblyPath = Path.Combine(Directories.LocalAppData.FullName, "assemblies"),
			};

			VirtualDesktop.Provider = provider;
			VirtualDesktop.Provider.Initialize().ContinueWith(Continue, TaskScheduler.FromCurrentSynchronizationContext());

			void Continue(Task t)
			{
				switch (t.Status)
				{
					case TaskStatus.RanToCompletion:
						this.VirtualDesktopInitialized?.Invoke();
						break;

					case TaskStatus.Canceled:
						this.VirtualDesktopInitializationCanceled?.Invoke();
						break;

					case TaskStatus.Faulted:
						this.VirtualDesktopInitializationFailed?.Invoke(t.Exception);
						break;
				}
			}
		}
	}
}
