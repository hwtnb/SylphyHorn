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
	using ActionRegister = Func<Func<ShortcutKey>, Action<IntPtr>, IDisposable>;

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
			this.RegisterActions(Settings.ShortcutKey, this._hookService.RegisterKeyAction);
			this.RegisterActions(Settings.MouseShortcut, this._hookService.RegisterMouseAction);
		}

		public TaskTrayIcon CreateTaskTrayIcon()
		{
			if (this._taskTrayIcon == null)
			{
				const string iconUri = "pack://application:,,,/SylphyHorn;Component/.assets/tasktray.dark.ico";
				const string lightIconUri = "pack://application:,,,/SylphyHorn;Component/.assets/tasktray.light.ico";

				if (!Uri.TryCreate(iconUri, UriKind.Absolute, out var uri)) return null;
				if (!Uri.TryCreate(lightIconUri, UriKind.Absolute, out var lightUri)) return null;

				var darkIcon = IconHelper.GetIconFromResource(uri);
				var lightIcon = IconHelper.GetIconFromResource(lightUri);
				var menus = new[]
				{
					new TaskTrayIconItem(Resources.TaskTray_Menu_Settings, this.ShowSettings, () => Application.Args.CanSettings),
					new TaskTrayIconItem(Resources.TaskTray_Menu_Exit, this._shutdownAction),
#if DEBUG
					new TaskTrayIconItem("Tasktray Icon Test", () => new TaskTrayTestWindow().Show()),
#endif
				};

				this._taskTrayIcon = new TaskTrayIcon(darkIcon, lightIcon, menus);
			}

			return this._taskTrayIcon;
		}

		private void ShowSettings()
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
						SettingsService.SynchronizeOnStartup();
						this.RegisterActions();
						this.RegisterVirtualDesktopEvents();
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

		private void RegisterVirtualDesktopEvents()
		{
			if (ProductInfo.IsNameSupportBuild)
			{
				VirtualDesktop.Renamed += (sender, args) =>
				{
					var desktop = args.Source;
					var index = desktop.Index;
					var names = Settings.General.DesktopNames.Value;

					if (index >= names.Count) SettingsService.ResizeListIfNeeded();

					var targetName = names[index];
					targetName.Value = args.NewName;

					LocalSettingsProvider.Instance.SaveAsync().Wait();
				};
			}

			var idCaches = VirtualDesktop.AllDesktops.Select(d => d.Id).ToArray();
			VirtualDesktop.Created += (sender, args) =>
			{
				SettingsService.ResizeListIfNeeded();

				LocalSettingsProvider.Instance.SaveAsync().Wait();
				idCaches = VirtualDesktop.AllDesktops.Select(d => d.Id).ToArray();
			};
			if (ProductInfo.IsWallpaperSupportBuild)
			{
				VirtualDesktop.Destroyed += (sender, args) =>
				{
					var destroyedIndex = Array.IndexOf(idCaches, args.Destroyed.Id);
					if (destroyedIndex < 0) return;
					var nameSettings = Settings.General.DesktopNames;
					for (var i = destroyedIndex; i + 1 < nameSettings.Count; ++i)
					{
						nameSettings.Value[i].Value = nameSettings.Value[i + 1].Value;
					}
					var positionSettings = Settings.General.DesktopBackgroundPositions;
					for (var i = destroyedIndex; i + 1 < positionSettings.Count; ++i)
					{
						positionSettings.Value[i].Value = positionSettings.Value[i + 1].Value;
					}
					SettingsService.ResizeListIfNeeded();

					LocalSettingsProvider.Instance.SaveAsync().Wait();
					idCaches = VirtualDesktop.AllDesktops.Select(d => d.Id).ToArray();
				};
			}
			else
			{
				VirtualDesktop.Destroyed += (sender, args) =>
				{
					var destroyedIndex = Array.IndexOf(idCaches, args.Destroyed.Id);
					if (destroyedIndex < 0) return;
					var nameSettings = Settings.General.DesktopNames;
					for (var i = destroyedIndex; i + 1 < nameSettings.Count; ++i)
					{
						nameSettings.Value[i].Value = nameSettings.Value[i + 1].Value;
					}
					var pathSettings = Settings.General.DesktopBackgroundImagePaths;
					for (var i = destroyedIndex; i + 1 < pathSettings.Count; ++i)
					{
						pathSettings.Value[i].Value = pathSettings.Value[i + 1].Value;
					}
					var positionSettings = Settings.General.DesktopBackgroundPositions;
					for (var i = destroyedIndex; i + 1 < positionSettings.Count; ++i)
					{
						positionSettings.Value[i].Value = positionSettings.Value[i + 1].Value;
					}
					SettingsService.ResizeListIfNeeded();

					LocalSettingsProvider.Instance.SaveAsync().Wait();
					idCaches = VirtualDesktop.AllDesktops.Select(d => d.Id).ToArray();
				};
				return;
			}

			VirtualDesktop.Moved += (sender, args) =>
			{
				SettingsService.SynchronizeWithWindows();
				Settings.General.DesktopBackgroundPositions.Move(args.OldIndex, args.NewIndex);

				LocalSettingsProvider.Instance.SaveAsync().Wait();
				idCaches = VirtualDesktop.AllDesktops.Select(d => d.Id).ToArray();
			};
			VirtualDesktop.WallpaperChanged += (sender, args) =>
			{
				var desktop = args.Source;
				var index = desktop.Index;
				var paths = Settings.General.DesktopBackgroundImagePaths.Value;

				if (index >= paths.Count) SettingsService.ResizeListIfNeeded();

				var targetPath = paths[index];
				targetPath.Value = args.NewPath;

				LocalSettingsProvider.Instance.SaveAsync().Wait();
			};
		}

		private void RegisterActions(ShortcutKeySettings settings, ActionRegister register)
		{
			register(() => settings.MoveLeft.ToShortcutKey(), hWnd => hWnd.MoveToLeft())
				.AddTo(this._disposable);

			register(() => settings.MoveLeftAndSwitch.ToShortcutKey(), hWnd => hWnd.MoveToLeft()?.Switch())
				.AddTo(this._disposable);

			register(() => settings.MoveRight.ToShortcutKey(), hWnd => hWnd.MoveToRight())
				.AddTo(this._disposable);

			register(() => settings.MoveRightAndSwitch.ToShortcutKey(), hWnd => hWnd.MoveToRight()?.Switch())
				.AddTo(this._disposable);

			register(() => settings.MoveNew.ToShortcutKey(), hWnd => hWnd.MoveToNew())
				.AddTo(this._disposable);

			register(() => settings.MoveNewAndSwitch.ToShortcutKey(), hWnd => hWnd.MoveToNew()?.Switch())
				.AddTo(this._disposable);

			register(() => settings.MoveToPrevious.ToShortcutKey(), hWnd => hWnd.MoveToPrevious())
				.AddTo(this._disposable);

			register(() => settings.MoveToPreviousAndSwitch.ToShortcutKey(), hWnd => hWnd.MoveToPrevious()?.Switch())
				.AddTo(this._disposable);

			var isKeyboardSettings = settings as MouseShortcutSettings == null;
			if (isKeyboardSettings)
			{
				if (Settings.General.OverrideWindowsDefaultKeyCombination)
				{
					register(() => settings.SwitchToLeftWithDefault.ToShortcutKey(), _ => { })
						.AddTo(this._disposable);

					register(() => settings.SwitchToRightWithDefault.ToShortcutKey(), _ => { })
						.AddTo(this._disposable);
				}
				else if (Settings.General.LoopDesktop)
				{
					register(
							() => settings.SwitchToLeftWithDefault.ToShortcutKey(),
							_ => VirtualDesktopService.GetLeft()?.Switch())
						.AddTo(this._disposable);

					register(
							() => settings.SwitchToRightWithDefault.ToShortcutKey(),
							_ => VirtualDesktopService.GetRight()?.Switch())
						.AddTo(this._disposable);
				}

				register(() => settings.SwitchToLeft.ToShortcutKey(), _ => VirtualDesktopService.GetLeft()?.Switch())
					.AddTo(this._disposable);

				register(() => settings.SwitchToRight.ToShortcutKey(), _ => VirtualDesktopService.GetRight()?.Switch())
					.AddTo(this._disposable);

				register(() => settings.SwitchToPrevious.ToShortcutKey(), _ => VirtualDesktopService.GetPrevious()?.Switch())
					.AddTo(this._disposable);
			}
			else
			{
				register(() => settings.SwitchToLeft.ToShortcutKey(), _ => VirtualDesktopService.GetLeft()?.Switch())
					.AddTo(this._disposable);

				register(() => settings.SwitchToRight.ToShortcutKey(), _ => VirtualDesktopService.GetRight()?.Switch())
					.AddTo(this._disposable);

				register(() => settings.SwitchToPrevious.ToShortcutKey(), _ => VirtualDesktopService.GetPrevious()?.Switch())
					.AddTo(this._disposable);
			}

			if (ProductInfo.IsReorderingSupportBuild)
			{
				register(() => settings.SwapDesktopLeft.ToShortcutKey(), _ => VirtualDesktopService.SwapCurrentForLeft())
					.AddTo(this._disposable);

				register(() => settings.SwapDesktopRight.ToShortcutKey(), _ => VirtualDesktopService.SwapCurrentForRight())
					.AddTo(this._disposable);

				register(() => settings.SwapDesktopFirst.ToShortcutKey(), _ => VirtualDesktopService.SwapCurrentForFirst())
					.AddTo(this._disposable);

				register(() => settings.SwapDesktopLast.ToShortcutKey(), _ => VirtualDesktopService.SwapCurrentForLast())
					.AddTo(this._disposable);
			}
			else
			{
				register(() => settings.SwapDesktopLeft.ToShortcutKey(), _ => { })
					.AddTo(this._disposable);

				register(() => settings.SwapDesktopRight.ToShortcutKey(), _ => { })
					.AddTo(this._disposable);

				register(() => settings.SwapDesktopFirst.ToShortcutKey(), _ => { })
					.AddTo(this._disposable);

				register(() => settings.SwapDesktopLast.ToShortcutKey(), _ => { })
					.AddTo(this._disposable);
			}

			register(() => settings.CloseAndSwitchLeft.ToShortcutKey(), _ => VirtualDesktopService.CloseAndSwitchLeft())
				.AddTo(this._disposable);

			register(() => settings.CloseAndSwitchRight.ToShortcutKey(), _ => VirtualDesktopService.CloseAndSwitchRight())
				.AddTo(this._disposable);

			register(() => settings.ShowTaskView.ToShortcutKey(), _ => VirtualDesktopService.ShowTaskView())
				.AddTo(this._disposable);

			register(() => settings.ShowWindowSwitch.ToShortcutKey(), _ => VirtualDesktopService.ShowWindowSwitch())
				.AddTo(this._disposable);

			register(() => settings.Pin.ToShortcutKey(), hWnd => hWnd.Pin())
				.AddTo(this._disposable);

			register(() => settings.Unpin.ToShortcutKey(), hWnd => hWnd.Unpin())
				.AddTo(this._disposable);

			register(() => settings.TogglePin.ToShortcutKey(), hWnd => hWnd.TogglePin())
				.AddTo(this._disposable);

			register(() => settings.PinApp.ToShortcutKey(), hWnd => hWnd.PinApp())
				.AddTo(this._disposable);

			register(() => settings.UnpinApp.ToShortcutKey(), hWnd => hWnd.UnpinApp())
				.AddTo(this._disposable);

			register(() => settings.TogglePinApp.ToShortcutKey(), hWnd => hWnd.TogglePinApp())
				.AddTo(this._disposable);

			register(() => settings.ShowSettings.ToShortcutKey(), _ =>
				{
					if (Application.Args.CanSettings) this.ShowSettings();
				})
				.AddTo(this._disposable);

			register(() => settings.ToggleDesktopNotification.ToShortcutKey(), _ => NotificationService.Instance.ToggleCurrentDesktop())
				.AddTo(this._disposable);

			var desktopCount = VirtualDesktopService.Count;
			var switchToIndices = settings.SwitchToIndices.Value;
			for (var index = 0; index < desktopCount && index < switchToIndices.Count; ++index)
			{
				RegisterSpecifiedDesktopSwitching(index, switchToIndices[index].ToShortcutKey());
			}

			var swapDesktopIndices = settings.SwapDesktopIndices.Value;
			for (var index = 0; index < desktopCount && index < swapDesktopIndices.Count; ++index)
			{
				RegisterSpecifiedDesktopSwapping(index, swapDesktopIndices[index].ToShortcutKey());
			}

			var moveToIndices = settings.MoveToIndices.Value;
			for (var index = 0; index < desktopCount && index < moveToIndices.Count; ++index)
			{
				RegisterMovingToSpecifiedDesktop(index, moveToIndices[index].ToShortcutKey());
			}

			var moveToIndicesAndSwitch = settings.MoveToIndicesAndSwitch.Value;
			for (var index = 0; index < desktopCount && index < moveToIndicesAndSwitch.Count; ++index)
			{
				RegisterMovingToSpecifiedDesktopAndSwitch(index, moveToIndicesAndSwitch[index].ToShortcutKey());
			}

			void RegisterSpecifiedDesktopSwitching(int i, ShortcutKey shortcut)
			{
				register(() => shortcut, _ => VirtualDesktopService.GetByIndex(i)?.Switch())
					.AddTo(this._disposable);
			};

			void RegisterSpecifiedDesktopSwapping(int i, ShortcutKey shortcut)
			{
				register(() => shortcut, _ => VirtualDesktopService.SwapCurrentByIndex(i))
					.AddTo(this._disposable);
			};

			void RegisterMovingToSpecifiedDesktop(int i, ShortcutKey shortcut)
			{
				register(() => shortcut, hWnd => hWnd.MoveToIndex(i))
					.AddTo(this._disposable);
			};

			void RegisterMovingToSpecifiedDesktopAndSwitch(int i, ShortcutKey shortcut)
			{
				register(() => shortcut, hWnd => hWnd.MoveToIndex(i)?.Switch())
					.AddTo(this._disposable);
			};
		}
	}
}
