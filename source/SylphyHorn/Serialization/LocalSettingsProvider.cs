using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using SylphyHorn.Properties;

namespace SylphyHorn.Serialization
{
	public sealed class LocalSettingsProvider : DictionaryProvider
	{
		public static readonly string SupportedFormats = "XML (*.xml)|*.xml";

		public static TimeSpan FileSystemHandlerThrottleDueTime { get; set; } = TimeSpan.FromMilliseconds(1500);

		public static LocalSettingsProvider Instance { get; } = new LocalSettingsProvider();

		private readonly FileInfo _targetFile;

		public bool Available { get; }

		public string FilePath => this._targetFile.FullName;

		private LocalSettingsProvider()
		{
			var path = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				ProductInfo.Company,
				ProductInfo.Product,
				this.Filename);

			var file = new FileInfo(path);
			if (file.Directory == null || file.DirectoryName == null)
			{
				this.Available = false;
				return;
			}

			if (!file.Directory.Exists)
			{
				file.Directory.Create();
			}

			this._targetFile = file;
			this.Available = true;
		}


		public async Task LoadOrMigrateAsync()
		{
			if (this.Available && this._targetFile.Exists)
			{
				await this.LoadAsync().ConfigureAwait(false);
				return;
			}

			var path = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				ProductInfo.OriginalCompany,
				ProductInfo.OriginalProduct,
				this.Filename);

			if (File.Exists(path))
			{
				await this.ImportAsync(path).ConfigureAwait(false);
				await this.SaveAsync().ConfigureAwait(false);
			}
			else
			{
				await this.LoadAsync().ConfigureAwait(false);
			}
		}

		protected override Task SaveAsyncCore(IDictionary<string, object> dic)
		{
			if (!this.Available)
			{
				return Task.FromResult<IDictionary<string, object>>(null);
			}

			return this.SaveAsyncCore(dic, this._targetFile);
		}

		protected override Task SaveAsyncCore(IDictionary<string, object> dic, string path)
		{
			var file = new FileInfo(path);
			if (file.Directory == null || file.DirectoryName == null)
			{
				return Task.FromResult<IDictionary<string, object>>(null);
			}

			if (!file.Directory.Exists)
			{
				file.Directory.Create();
			}

			return this.SaveAsyncCore(dic, file);
		}

		private Task SaveAsyncCore(IDictionary<string, object> dic, FileInfo targetFile)
		{
			return Task.Run(() =>
			{
				var serializer = new DataContractSerializer(dic.GetType(), this.KnownTypes);
				var settings = new XmlWriterSettings
				{
					Indent = true, // more readable!!!
				};

				using (var stream = targetFile.Open(FileMode.Create))
				using (var writer = XmlWriter.Create(stream, settings))
				{
					serializer.WriteObject(writer, dic);
				}
			});
		}

		protected override Task<IDictionary<string, object>> LoadAsyncCore()
		{
			if (!this.Available)
			{
				return Task.FromResult<IDictionary<string, object>>(null);
			}

			return this.LoadAsyncCore(this._targetFile);
		}

		protected override Task<IDictionary<string, object>> LoadAsyncCore(string path)
		{
			var file = new FileInfo(path);
			if (file.Directory == null || file.DirectoryName == null)
			{
				return Task.FromResult<IDictionary<string, object>>(null);
			}

			return this.LoadAsyncCore(file);
		}

		private Task<IDictionary<string, object>> LoadAsyncCore(FileInfo targetFile)
		{
			if (!targetFile.Exists)
			{
				return Task.FromResult<IDictionary<string, object>>(null);
			}

			return Task.Run(() =>
			{
				if (!targetFile.Exists) return null;

				var serializer = new DataContractSerializer(typeof(IDictionary<string, object>), this.KnownTypes);

				using (var stream = targetFile.OpenRead())
				{
					return serializer.ReadObject(stream) as IDictionary<string, object>;
				}
			});
		}
	}
}
