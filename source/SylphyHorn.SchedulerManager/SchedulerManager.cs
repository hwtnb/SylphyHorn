using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using TaskScheduler;

namespace SylphyHorn
{
	class SchedulerManager
	{
		static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Environment.Exit(-1);
			}
			var command = args[0];
			var process = args.Length > 1 ? new SchedulerProcess(appPath: args[1]) : new SchedulerProcess();
			switch (command)
			{
				case "register":
					Environment.Exit(process.Register());
					break;
				case "unregister":
					Environment.Exit(process.Unregister());
					break;
				case "start":
					Environment.Exit(process.Start());
					break;
				case "stop":
					Environment.Exit(process.Stop());
					break;
				case "restart":
					Environment.Exit(process.Restart());
					break;
				case "hastask":
					Environment.Exit(Convert.ToInt32(process.HasTask));
					break;
				case "isrunning":
					Environment.Exit(Convert.ToInt32(process.IsRunning));
					break;
			}

		}
	}

	class SchedulerProcess
	{
		private const string _schedulerDir = "\\";
		private const string _defaultAppName = "SylphyHorn";
		private const string _defaultAppExtension = ".exe";
		private readonly string _schedulerPath;
		private readonly string _taskName;
		private readonly string _appPath;
		private readonly string _appName;
		private readonly WindowsPrincipal _principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

		public bool HasTask => this.FindStartupTask() != null;

		public bool IsRunning => this.FindStartupTask()?.State == _TASK_STATE.TASK_STATE_RUNNING;

		public bool IsAdministrator => _principal.IsInRole(WindowsBuiltInRole.Administrator);

		public SchedulerProcess()
			: this(GetDefaultApplicationPath())
		{
		}

		public SchedulerProcess(string appPath)
		{
			if (string.IsNullOrEmpty(appPath) || !File.Exists(appPath))
			{
				throw new ArgumentNullException($"Error: app path ({appPath}) is invalid.");
			}
			this._appPath = $"\"{appPath}\"";
			this._appName = Path.GetFileNameWithoutExtension(appPath);
			this._taskName = this._appName + " Startup";
			this._schedulerPath = _schedulerDir + this._taskName;
		}

		public int Register()
		{
			if (!this.IsAdministrator)
			{
				return -1;
			}

			ITaskService taskService = null;
			try
			{
				taskService = new TaskScheduler.TaskScheduler();
				taskService.Connect(null, null, null, null);
				ITaskFolder rootfolder = null;
				try
				{
					rootfolder = taskService.GetFolder(_schedulerDir);
					var taskDefinition = this.CreateTaskDefinition(taskService);
					try
					{
						rootfolder.RegisterTaskDefinition(
							this._schedulerPath,
							taskDefinition,
							(int)_TASK_CREATION.TASK_CREATE_OR_UPDATE,
							null,
							null,
							_TASK_LOGON_TYPE.TASK_LOGON_NONE,
							null
						);
					}
					catch (UnauthorizedAccessException)
					{
						return -1;
					}
					catch (Exception e)
					{
						return e.HResult;
					}
				}
				finally
				{
					if (rootfolder != null) Marshal.ReleaseComObject(rootfolder);
				}
			}
			finally
			{
				if (taskService != null) Marshal.ReleaseComObject(taskService);
			}

			return 0;
		}

		public int Unregister()
		{
			if (!this.IsAdministrator)
			{
				return -1;
			}
			else if (!this.HasTask)
			{
				return 0;
			}

			ITaskService taskService = null;
			try
			{
				taskService = new TaskScheduler.TaskScheduler();
				taskService.Connect(null, null, null, null);
				ITaskFolder rootfolder = null;
				try
				{
					rootfolder = taskService.GetFolder(_schedulerDir);
					try
					{
						rootfolder.DeleteTask(this._taskName, 0);
					}
					catch (UnauthorizedAccessException)
					{
						return -1;
					}
					catch (Exception e)
					{
						return e.HResult;
					}
				}
				finally
				{
					if (rootfolder != null) Marshal.ReleaseComObject(rootfolder);
				}
			}
			finally
			{
				if (taskService != null) Marshal.ReleaseComObject(taskService);
			}

			return 0;
		}

		public int Start()
		{
			var task = this.FindStartupTask();
			if (task == null)
			{
				return -1;
			}
			task.Run(null);
			return 0;
		}

		public int Stop()
		{
			var task = this.FindStartupTask();
			if (task == null)
			{
				return -1;
			}
			task.Stop(0);
			return 0;
		}

		public int Restart()
		{
			var task = this.FindStartupTask();
			if (task == null || task.State == _TASK_STATE.TASK_STATE_DISABLED)
			{
				return -1;
			}

			var taskActions = task.Definition.Actions;
			var appPath = taskActions.Count > 0
				? ((IExecAction)taskActions[1])?.Path ?? this._appPath
				: this._appPath;
			appPath = Regex.Replace(appPath, @"^""(.+)""$", "$1");
			if (string.IsNullOrEmpty(appPath) || !File.Exists(appPath))
			{
				throw new FileNotFoundException($"Error: app path ({appPath}) is invalid.");
			}
			else if (task.State == _TASK_STATE.TASK_STATE_RUNNING)
			{
				task.Stop(0);
			}

			var appName = Path.GetFileNameWithoutExtension(appPath);
			var totalTime = 0;
			const int timeout = 30000;
			const int interval = 100;
			while (Process.GetProcessesByName(appName).Length > 0)
			{
				Thread.Sleep(interval);
				totalTime += interval;
				if (totalTime >= timeout)
				{
					return -1;
				}
			}

			task.Run(null);
			return 0;
		}

		private ITaskDefinition CreateTaskDefinition(ITaskService taskService)
		{
			var taskDefinition = taskService.NewTask(0);
			var registrationInfo = taskDefinition.RegistrationInfo;

			// Actions Property
			var actionCollection = taskDefinition.Actions;
			var execAction = (IExecAction)actionCollection.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);

			// Triggers Property
			var triggerCollection = taskDefinition.Triggers;
			var trigger = (ILogonTrigger)triggerCollection.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_LOGON);

			// Settings Property
			var taskSettings = taskDefinition.Settings;

			/* General Settings */
			registrationInfo.Author = "";
			registrationInfo.Description = "";
			var principal = taskDefinition.Principal;
			//principal.UserId = Environment.UserDomainName + "\\" + Environment.UserName;
			principal.LogonType = _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN;
			principal.RunLevel = _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;
			taskSettings.DisallowStartIfOnBatteries = false;
			taskSettings.ExecutionTimeLimit = "PT0S";
			taskSettings.Compatibility = _TASK_COMPATIBILITY.TASK_COMPATIBILITY_V2;
			taskSettings.Hidden = false;
			taskSettings.Priority = 6;

			/* Trigger Settings */
			trigger.Enabled = true;

			/* Operation Settings */
			execAction.Path = this._appPath;
			//execAction.Arguments = "";

			return taskDefinition;
		}

		private IRegisteredTask FindStartupTask()
		{
			ITaskService taskService = null;
			try
			{
				taskService = new TaskScheduler.TaskScheduler();
				taskService.Connect(null, null, null, null);
				ITaskFolder rootfolder = null;
				try
				{
					rootfolder = taskService.GetFolder(_schedulerDir);
					try
					{
						return rootfolder.GetTask(this._schedulerPath);
					}
					catch (FileNotFoundException)
					{
						return null;
					}
					catch (UnauthorizedAccessException)
					{
						return null;
					}
					catch (Exception)
					{
						return null;
					}
				}
				finally
				{
					if (rootfolder != null) Marshal.ReleaseComObject(rootfolder);
				}
			}
			finally
			{
				if (taskService != null) Marshal.ReleaseComObject(taskService);
			}
		}

		private static string GetDefaultApplicationPath()
		{
			var appDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			return Path.Combine(appDir, SchedulerProcess._defaultAppName + SchedulerProcess._defaultAppExtension);
		}
	}
}
