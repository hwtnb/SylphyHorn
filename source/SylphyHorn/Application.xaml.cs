using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Livet;
using MetroRadiance.UI;
using MetroTrilithon.Lifetime;
using MetroTrilithon.Threading.Tasks;
using SylphyHorn.Properties;
using SylphyHorn.Serialization;
using SylphyHorn.Services;
using SylphyHorn.UI;

namespace SylphyHorn
{
	sealed partial class Application : IDisposableHolder
	{
		public static bool IsWindowsBridge { get; }
#if APPX
			= true;
#else
			= false;
#endif

		private readonly LivetCompositeDisposable _compositeDisposable = new LivetCompositeDisposable();

		internal HookService HookService { get; private set; }

		internal TaskTrayIcon TaskTrayIcon { get; private set; }

		protected override void OnStartup(StartupEventArgs e)
		{
			Args = new CommandLineArgs(e.Args);

			if (Args.Setup)
			{
				this.SetupShortcut();
			}

			if (!this.WaitUntilExplorerStarts())
			{
				MessageBox.Show("This application must start after Explorer is launched.", "Not ready", MessageBoxButton.OK, MessageBoxImage.Stop);
				this.Shutdown();
				return;
			}

#if !DEBUG
			var appInstance = new MetroTrilithon.Desktop.ApplicationInstance().AddTo(this);
			if (appInstance.IsFirst || Args.Restarted.HasValue)
#endif
			{
				if (ProductInfo.OSBuild >= 14393)
				{
					this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
					DispatcherHelper.UIDispatcher = this.Dispatcher;

					this.DispatcherUnhandledException += this.HandleDispatcherUnhandledException;
					TaskLog.Occured += (sender, log) => LoggingService.Instance.Register(log);

					LocalSettingsProvider.Instance.LoadOrMigrateAsync().Wait();

					Settings.General.Culture.Subscribe(x => ResourceService.Current.ChangeCulture(x)).AddTo(this);
					ThemeService.Current.Register(this, Theme.Windows, Accent.Windows);

					this.HookService = new HookService().AddTo(this);

					var preparation = new ApplicationPreparation(this.HookService, this.Shutdown, this);
					this.TaskTrayIcon = preparation.CreateTaskTrayIcon().AddTo(this);

					if (Settings.General.FirstTime)
					{
						preparation.CreateFirstTimeBaloon().Show();

						Settings.General.FirstTime.Value = false;
						LocalSettingsProvider.Instance.SaveAsync().Forget();
					}

					preparation.VirtualDesktopInitialized += () =>
					{
						this.TaskTrayIcon.Show();
						this.TaskTrayIcon.Reload();
						if (Settings.General.AlwaysShowDesktopNotification)
						{
							NotificationService.Instance.ShowCurrentDesktop();
						}
					};
					preparation.VirtualDesktopInitializationCanceled += () => this.Shutdown(); // ToDo
					preparation.VirtualDesktopInitializationFailed += ex =>
					{
						this.TaskTrayIcon.Show();
						LoggingService.Instance.Register(ex);
						this.RestartOrShutdown("Virtual desktop initialization is failed.", "Virtual Desktop Initialization Failed");
					};
					preparation.PrepareVirtualDesktop();

					NotificationService.Instance.AddTo(this);
					WallpaperService.Instance.AddTo(this);

#if !DEBUG
					appInstance.CommandLineArgsReceived += (sender, message) =>
					{
						var args = new CommandLineArgs(message.CommandLineArgs);
						if (args.Setup) this.SetupShortcut();
					};
#endif

					base.OnStartup(e);
				}
				else
				{
					MessageBox.Show("This application is supported on Windows 10 Anniversary Update (build 14393) or later.", "Not supported", MessageBoxButton.OK, MessageBoxImage.Stop);
					this.Shutdown();
				}
			}
#if !DEBUG
			else
			{
				appInstance.SendCommandLineArgs(e.Args);
				this.Shutdown();
			}
#endif
		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
			((IDisposable)this).Dispose();
		}

		private void SetupShortcut()
		{
			var startup = new Startup();
			if (!startup.IsExists)
			{
				startup.Create();
			}
		}

		private void HandleDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
		{
			LoggingService.Instance.Register(args.Exception);
			args.Handled = true;
		}

		private bool WaitUntilExplorerStarts()
		{
			const string explorerProcessName = "explorer";
			if (Process.GetProcessesByName(explorerProcessName).Length > 0)
			{
				return true;
			}

			const int tryCount = 5;
			const int timeout = 5000;
			const int interval = timeout / tryCount;
			for (var i = 0; i < tryCount; ++i)
			{
				Thread.Sleep(interval);
				if (Process.GetProcessesByName(explorerProcessName).Length > 0)
				{
					return true;
				}
			}
			return false;
		}

		private void RestartOrShutdown(string message, string caption)
		{
			var result = MessageBox.Show(
				$"{message}\n\nDo you want to restart {ProductInfo.Title} now?",
				caption,
				MessageBoxButton.YesNo,
				MessageBoxImage.Stop
			);
			if (result == MessageBoxResult.Yes)
			{
				try
				{
					Restart();
				}
				catch (Exception ex)
				{
					LoggingService.Instance.Register(ex);
					this.Shutdown();
					return;
				}
			}
			this.Shutdown();
		}

		#region IDisposable members

		ICollection<IDisposable> IDisposableHolder.CompositeDisposable => this._compositeDisposable;

		void IDisposable.Dispose()
		{
			this._compositeDisposable.Dispose();
		}

		#endregion
	}
}
