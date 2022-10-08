using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetroTrilithon.Serialization;

namespace SylphyHorn.Serialization
{
	public abstract class DictionaryProvider : ISerializationProvider
	{
		private readonly object _sync = new object();
		private Dictionary<string, object> _settings = new Dictionary<string, object>();

		public bool IsLoaded { get; private set; }

		public virtual string Filename { get; } = "Settings.xml";

		public virtual Type[] KnownTypes { get; } = { typeof(bool), typeof(int[]), };

		public event EventHandler Reloaded;

		public void SetValue<T>(string key, T value)
		{
			lock (this._sync)
			{
				this._settings[key] = value;
			}
		}

		public bool TryGetValue<T>(string key, out T value)
		{
			lock (this._sync)
			{
				if (this._settings.TryGetValue(key, out var obj) && obj is T t)
				{
					value = t;
					return true;
				}
			}

			value = default(T);
			return false;
		}

		public bool RemoveValue(string key)
		{
			lock (this._sync)
			{
				return this._settings.Remove(key);
			}
		}

		public void Clear()
		{
			lock (this._sync)
			{
				this._settings.Clear();
				this.OnReloaded();
			}
		}


		void ISerializationProvider.Save()
		{
			this.SaveAsync().Wait();
		}

		public async Task SaveAsync()
		{
			try
			{
				SortedDictionary<string, object> current;

				lock (this._sync)
				{
					current = new SortedDictionary<string, object>(this._settings);
				}

				await this.SaveAsyncCore(current).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}
		}

		public async Task ExportAsync(string path)
		{
			try
			{
				SortedDictionary<string, object> current;

				lock (this._sync)
				{
					current = new SortedDictionary<string, object>(this._settings);
				}

				await this.SaveAsyncCore(current, path).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}
		}

		protected abstract Task SaveAsyncCore(IDictionary<string, object> dic);

		protected abstract Task SaveAsyncCore(IDictionary<string, object> dic, string path);

		void ISerializationProvider.Load()
		{
			this.LoadAsync().Wait();
		}

		public async Task LoadAsync()
		{
			try
			{
				var dic = await this.LoadAsyncCore().ConfigureAwait(false);

				lock (this._sync)
				{
					this._settings = dic != null
						? new Dictionary<string, object>(dic)
						: new Dictionary<string, object>();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}

			this.IsLoaded = true;
		}

		public async Task ImportAsync(string path)
		{
			try
			{
				var dic = await this.LoadAsyncCore(path).ConfigureAwait(false);

				lock (this._sync)
				{
					this._settings = dic != null
						? new Dictionary<string, object>(dic)
						: new Dictionary<string, object>();
					this.OnReloaded();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}

			this.IsLoaded = true;
		}

		protected abstract Task<IDictionary<string, object>> LoadAsyncCore();

		protected abstract Task<IDictionary<string, object>> LoadAsyncCore(string path);

		protected void OnReloaded()
		{
			this.Reloaded?.Invoke(this, EventArgs.Empty);
		}
	}
}
