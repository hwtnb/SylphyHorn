using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using SylphyHorn.Interop;

namespace SylphyHorn
{
	public class StartupScheduler
	{
		private const string _managerName = "SchedulerManager";
		private const int _waitingTime = 20000;
		private readonly string _appPath;
		private readonly string _managerPath;
		private readonly WindowsPrincipal _principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

		public bool IsExists => this.HasStartupTask();

		public bool IsRunning => this.IsTaskRunning();

		public bool IsEnabled => !Platform.IsUwp;

		public bool IsAdministrator => _principal.IsInRole(WindowsBuiltInRole.Administrator);

		public StartupScheduler()
			: this(Assembly.GetExecutingAssembly().Location)
		{
		}

		public StartupScheduler(string appPath)
		{
			this._appPath = appPath;
			this._managerPath = Path.Combine(Path.GetDirectoryName(appPath), _managerName + ".exe");
		}

		public void Register()
		{
			if (Platform.IsUwp)
			{
				return;
			}

			var processInfo = this.CreateStartInfo(isRequiredAdmin: true);
			processInfo.Arguments = "register " + $"\"{this._appPath}\"";

			try
			{
				using (var process = Process.Start(processInfo))
				{ 
					process.WaitForExit(_waitingTime);
					if (!process.HasExited)
					{
						process.Kill();
					}
					var exitCode = process.ExitCode;
					if (exitCode == -1)
					{
						throw new UnauthorizedAccessException();
					}
					else if (exitCode != 0)
					{
						throw new Exception(exitCode.ToString());
					}
				}
			}
			catch (System.ComponentModel.Win32Exception)
			{
				return;
			}
		}

		public void Unregister()
		{
			if (Platform.IsUwp || !this.IsExists)
			{
				return;
			}

			var processInfo = this.CreateStartInfo(isRequiredAdmin: true);
			processInfo.Arguments = "unregister " + $"\"{this._appPath}\"";

			try
			{
				using (var process = Process.Start(processInfo))
				{ 
					process.WaitForExit(_waitingTime);
					if (!process.HasExited)
					{
						process.Kill();
					}
					var exitCode = process.ExitCode;
					if (exitCode == -1)
					{
						throw new UnauthorizedAccessException();
					}
					else if (exitCode != 0)
					{
						throw new Exception(exitCode.ToString());
					}
				}
			}
			catch (System.ComponentModel.Win32Exception)
			{
				return;
			}
		}

		public void Restart()
		{
			if (Platform.IsUwp || !this.IsExists)
			{
				return;
			}

			var processInfo = this.CreateStartInfo(isRequiredAdmin: true);
			processInfo.Arguments = "restart " + $"\"{this._appPath}\"";

			try
			{
				using (var process = Process.Start(processInfo))
				{ 
					process.WaitForExit(_waitingTime);
					if (!process.HasExited)
					{
						process.Kill();
					}
					var exitCode = process.ExitCode;
					if (exitCode == -1)
					{
						throw new UnauthorizedAccessException();
					}
					else if (exitCode != 0)
					{
						throw new Exception(exitCode.ToString());
					}
				}
			}
			catch (System.ComponentModel.Win32Exception)
			{
				return;
			}
		}

		private bool HasStartupTask()
		{
			if (Platform.IsUwp)
			{
				return false;
			}

			var processInfo = this.CreateStartInfo(isRequiredAdmin: false);
			processInfo.Arguments = "hastask " + $"\"{this._appPath}\"";

			try
			{
				using (var process = Process.Start(processInfo))
				{ 
					process.WaitForExit(_waitingTime);
					if (!process.HasExited)
					{
						process.Kill();
						return false;
					}
					return Convert.ToBoolean(process.ExitCode);
				}
			}
			catch (System.ComponentModel.Win32Exception)
			{
				return false;
			}
		}

		private bool IsTaskRunning()
		{
			if (Platform.IsUwp)
			{
				return false;
			}

			var processInfo = this.CreateStartInfo(isRequiredAdmin: false);
			processInfo.Arguments = "isrunning " + $"\"{this._appPath}\"";

			try
			{
				using (var process = Process.Start(processInfo))
				{
					process.WaitForExit(_waitingTime);
					if (!process.HasExited)
					{
						process.Kill();
						return false;
					}
					return Convert.ToBoolean(process.ExitCode);
				}
			}
			catch (System.ComponentModel.Win32Exception)
			{
				return false;
			}
		}

		private ProcessStartInfo CreateStartInfo(bool isRequiredAdmin)
		{
			var processInfo = new ProcessStartInfo();
			processInfo.UseShellExecute = true;
			processInfo.CreateNoWindow = true;
			processInfo.WindowStyle = ProcessWindowStyle.Hidden;
			processInfo.FileName = _managerPath;
			if (isRequiredAdmin) {
				processInfo.Verb = "runas";
			}
			return processInfo;
		}
	}
}
