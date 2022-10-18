using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using MetroTrilithon.Linq;
using MetroTrilithon.Serialization;

namespace SylphyHorn.Serialization
{
	public class SerializableProperty<T> : SerializablePropertyBase<T>
	{
		public SerializableProperty(string key, ISerializationProvider provider) : base(key, provider) { }
		public SerializableProperty(string key, ISerializationProvider provider, T defaultValue) : base(key, provider, defaultValue) { }
	}


	public class IndexedSerializableProperty<T> : SerializablePropertyBase<T>
	{
		public int Index { get; private set; } = 0;
		public string NumberText => (this.Index + 1).ToString();

		public IndexedSerializableProperty(string key, ISerializationProvider provider) : base(key, provider) { }
		public IndexedSerializableProperty(string key, ISerializationProvider provider, T defaultValue) : base(key, provider, defaultValue) { }

		public IndexedSerializableProperty(string key, int index, ISerializationProvider provider) : base(key, provider)
		{
			this.Index = index;
		}

		public IndexedSerializableProperty(string key, int index, ISerializationProvider provider, T defaultValue) : base(key, provider, defaultValue)
		{
			this.Index = index;
		}
	}


	public class ShortcutkeyProperty : IndexedSerializableProperty<IList<int>>
	{
		private const string _emptyString = "(none)";

		public ShortcutkeyProperty(string key, ISerializationProvider provider) : base(key, provider) { }
		public ShortcutkeyProperty(string key, ISerializationProvider provider, params int[] defaultValue) : base(key, provider, defaultValue) { }
		public ShortcutkeyProperty(string key, int index, ISerializationProvider provider) : base(key, index, provider) { }
		public ShortcutkeyProperty(string key, int index, ISerializationProvider provider, params int[] defaultValue) : base(key, index, provider, defaultValue) { }

		protected override object SerializeCore(IList<int> value)
		{
			if (value == null || value.Count == 0) return _emptyString;

			return value
				.Select(x => x.ToString(CultureInfo.InvariantCulture))
				.JoinString(",");
		}

		protected override IList<int> DeserializeCore(object value)
		{
			var data = value as string;
			if (data == null) return base.DeserializeCore(value);

			if (string.IsNullOrEmpty(data)) return null;
			if (string.Equals(data, _emptyString, StringComparison.OrdinalIgnoreCase)) return Array.Empty<int>();

			return data.Split(',')
				.Select(x => int.Parse(x))
				.ToList();
		}
	}


	public class DesktopNameProperty : IndexedSerializableProperty<string>
	{
		public DesktopNameProperty(string key, int index, ISerializationProvider provider) : base(key, index, provider) { }
		public DesktopNameProperty(string key, int index, ISerializationProvider provider, string defaultValue) : base(key, index, provider, defaultValue) { }
	}


	public class WallpaperPathProperty : IndexedSerializableProperty<string>
	{
		public WallpaperPathProperty(string key, int index, ISerializationProvider provider) : base(key, index, provider) { }
		public WallpaperPathProperty(string key, int index, ISerializationProvider provider, string defaultValue) : base(key, index, provider, defaultValue) { }
	}


	public class WallpaperPositionsProperty : IndexedSerializableProperty<byte>
	{
		public WallpaperPositionsProperty(string key, int index, ISerializationProvider provider) : base(key, index, provider, 4 /* WallpaperPosition.Fill */) { }
		public WallpaperPositionsProperty(string key, int index, ISerializationProvider provider, byte defaultValue) : base(key, index, provider, defaultValue) { }
	}


	public abstract class SerializablePropertyListBase<T> : INotifyPropertyChanged
	{
		private IReadOnlyList<T> _value;

		private SerializableProperty<int> _serializableCount;

		public string Key { get; }

		public ISerializationProvider Provider { get; }

		public IReadOnlyList<T> Value
		{
			get
			{
				return this._value;
			}
			set
			{
				if (!this.Provider.IsLoaded)
				{
					this.Provider.Load();
				}

				var isValueChanged = this._value?.Count != value?.Count && (!this._value?.SequenceEqual(value) ?? value != null);
				this._value = value;

				if (isValueChanged)
				{
					this._serializableCount.Value = value.Count;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Value)));
				}
			}
		}

		public int Count => this._value.Count;

		public event PropertyChangedEventHandler PropertyChanged;

		public SerializablePropertyListBase(string key, ISerializationProvider provider)
		{
			this.Key = key ?? throw new ArgumentNullException(nameof(key));
			this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));

			this.LoadMetaProperty();
			this.LoadProperties();

			this.Provider.Reloaded += this.ProviderOnReloaded;
		}

		public SerializablePropertyListBase(string key, int size, ISerializationProvider provider)
		{
			this.Key = key ?? throw new ArgumentNullException(nameof(key));
			this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));

			this.LoadMetaProperty();
			this.AddNewProperties(size);

			this.Provider.Reloaded += this.ProviderOnReloaded;
		}

		public SerializablePropertyListBase(string key, ISerializationProvider provider, params T[] defaultValues)
		{
			if (defaultValues == null) defaultValues = Array.Empty<T>();

			this.Key = key ?? throw new ArgumentNullException(nameof(key));
			this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));

			this.LoadMetaProperty();
			this.FillNewPropertiesWithDefaultValues(defaultValues);

			this.Provider.Reloaded += this.ProviderOnReloaded;
		}

		public void Resize(int size)
		{
			var oldValue = this.Value;
			var oldCount = oldValue.Count;
			if (oldCount > size)
			{
				this.Value = oldValue.ToList().GetRange(0, size);
				var provider = this.Provider;
				for (var i = size; i < oldCount; ++i)
				{
					var key = this.CreateItemName(i);
					provider.RemoveValue(key);
				}
			}
			else if (oldCount < size)
			{
				this.AddNewProperties(size);
			}
		}

		public void ResizeIfEmpty(int size)
		{
			var oldValue = this.Value;
			var oldCount = oldValue.Count;
			if (oldCount > size)
			{
				var newSize = 0;
				for (var i = oldCount - 1; i >= 0; --i)
				{
					if (!this.IsEmptyValue(oldValue[i]))
					{
						newSize = i + 1;
						break;
					}
				}
				if (newSize > size) size = newSize;
			}
			this.Resize(size);
		}

		public void StretchTo(int size)
		{
			var oldValue = this.Value;
			var oldCount = oldValue.Count;
			if (oldCount < size)
			{
				this.AddNewProperties(size);
			}
		}

		public void Move(int fromIndex, int toIndex)
		{
			if (fromIndex < 0 || toIndex < 0) throw new ArgumentOutOfRangeException();
			if (fromIndex >= Count || toIndex >= Count) throw new ArgumentOutOfRangeException();
			if (fromIndex == toIndex) return;

			this.MoveCore(fromIndex, toIndex);
		}

		protected void AddNewProperties(int newSize)
		{
			var oldValue = this.Value ?? new List<T>(newSize);
			var newValue = oldValue.ToList();
			for (var i = oldValue.Count; i < newSize; ++i)
			{
				newValue.Add(this.CreateProperty(i));
			}
			this.Value = newValue;
		}

		protected void FillNewPropertiesWithDefaultValues(T[] defaultValues)
		{
			var value = new List<T>(defaultValues.Length);
			for (var i = 0; i < defaultValues.Length; ++i)
			{
				value.Add(this.CreatePropertyWithDefault(i, defaultValues[i]));
			}
			this.Value = value;
		}

		protected abstract void MoveCore(int fromIndex, int toIndex);
		protected abstract void LoadProperties();
		protected abstract T CreateProperty(int index);
		protected abstract T CreatePropertyWithDefault(int index, T defaultValue);
		protected abstract bool IsEmptyValue(T value);

		protected void LoadPropertiesCore<U>()
		{
			var newValue = new List<T>();
			var provider = this.Provider;
			var index = 0;
			var maxCount = this._serializableCount?.Value ?? 0;
			var key = this.CreateItemName(index);
			while (provider.TryGetValue<U>(key, out _) || index < maxCount)
			{
				newValue.Add(this.CreateProperty(index));
				key = this.CreateItemName(++index);
			}
			this.Value = newValue;
		}

		protected void LoadMetaProperty()
		{
			var provider = this.Provider;
			var key = this.CreateCountKey();
			this._serializableCount = new SerializableProperty<int>(key, provider, 0);
			if (provider.TryGetValue<int>(key, out var value))
			{
				this._serializableCount.Value = value;
			}
		}

		protected string CreateItemName(int index)
		{
			return $"{this.Key}[{index}]";
		}

		protected string CreateCountKey()
		{
			return $"{this.Key}#Count";
		}

		protected void ProviderOnReloaded(object sender, EventArgs args)
		{
			this.LoadProperties();
		}
	}


	public class ShortcutkeyPropertyList : SerializablePropertyListBase<ShortcutkeyProperty>
	{
		public ShortcutkeyPropertyList(string key, ISerializationProvider provider) : base(key, provider) { }
		public ShortcutkeyPropertyList(string key, int size, ISerializationProvider provider) : base(key, size, provider) { }
		public ShortcutkeyPropertyList(string key, ISerializationProvider provider, params ShortcutkeyProperty[] defaultValues) : base(key, provider, defaultValues) { }

		protected override void MoveCore(int fromIndex, int toIndex)
		{
			var newValue = this.Value.ToList();
			var tempItem = this.Value[fromIndex].Value;
			if (fromIndex < toIndex)
			{
				var targetIndex = fromIndex;
				var sourceIndex = fromIndex + 1;
				for (; sourceIndex <= toIndex; ++targetIndex, ++sourceIndex)
				{
					newValue[targetIndex].Value = newValue[sourceIndex].Value;
				}
			}
			else
			{
				var targetIndex = fromIndex;
				var sourceIndex = fromIndex - 1;
				for (; sourceIndex >= toIndex; --targetIndex, --sourceIndex)
				{
					newValue[targetIndex].Value = newValue[sourceIndex].Value;
				}
			}
			newValue[toIndex].Value = tempItem;
			this.Value = newValue;
		}

		protected override void LoadProperties()
		{
			this.LoadPropertiesCore<string>();
		}

		protected override ShortcutkeyProperty CreateProperty(int index)
		{
			return new ShortcutkeyProperty(this.CreateItemName(index), index, this.Provider);
		}

		protected override ShortcutkeyProperty CreatePropertyWithDefault(int index, ShortcutkeyProperty defaultValue)
		{
			return new ShortcutkeyProperty(this.CreateItemName(index), index, this.Provider, defaultValue.Value.ToArray());
		}

		protected override bool IsEmptyValue(ShortcutkeyProperty value)
		{
			return value == null || value.Value == null || value.Value.Count == 0;
		}
	}


	public class DesktopNamePropertyList : SerializablePropertyListBase<DesktopNameProperty>
	{
		public DesktopNamePropertyList(string key, ISerializationProvider provider) : base(key, provider) { }
		public DesktopNamePropertyList(string key, int size, ISerializationProvider provider) : base(key, size, provider) { }
		public DesktopNamePropertyList(string key, ISerializationProvider provider, params DesktopNameProperty[] defaultValues) : base(key, provider, defaultValues) { }

		protected override void MoveCore(int fromIndex, int toIndex)
		{
			var newValue = this.Value.ToList();
			var tempItem = this.Value[fromIndex].Value;
			if (fromIndex < toIndex)
			{
				var targetIndex = fromIndex;
				var sourceIndex = fromIndex + 1;
				for (; sourceIndex <= toIndex; ++targetIndex, ++sourceIndex)
				{
					newValue[targetIndex].Value = newValue[sourceIndex].Value;
				}
			}
			else
			{
				var targetIndex = fromIndex;
				var sourceIndex = fromIndex - 1;
				for (; sourceIndex >= toIndex; --targetIndex, --sourceIndex)
				{
					newValue[targetIndex].Value = newValue[sourceIndex].Value;
				}
			}
			newValue[toIndex].Value = tempItem;
			this.Value = newValue;
		}

		protected override void LoadProperties()
		{
			this.LoadPropertiesCore<string>();
		}

		protected override DesktopNameProperty CreateProperty(int index)
		{
			return new DesktopNameProperty(this.CreateItemName(index), index, this.Provider);
		}

		protected override DesktopNameProperty CreatePropertyWithDefault(int index, DesktopNameProperty defaultValue)
		{
			return new DesktopNameProperty(this.CreateItemName(index), index, this.Provider, defaultValue.Value);
		}

		protected override bool IsEmptyValue(DesktopNameProperty value)
		{
			return value == null || string.IsNullOrEmpty(value.Value);
		}
	}


	public class WallpaperPathPropertyList : SerializablePropertyListBase<WallpaperPathProperty>
	{
		public WallpaperPathPropertyList(string key, ISerializationProvider provider) : base(key, provider) { }
		public WallpaperPathPropertyList(string key, int size, ISerializationProvider provider) : base(key, size, provider) { }
		public WallpaperPathPropertyList(string key, ISerializationProvider provider, params WallpaperPathProperty[] defaultValues) : base(key, provider, defaultValues) { }

		protected override void MoveCore(int fromIndex, int toIndex)
		{
			var newValue = this.Value.ToList();
			var tempItem = this.Value[fromIndex].Value;
			if (fromIndex < toIndex)
			{
				var targetIndex = fromIndex;
				var sourceIndex = fromIndex + 1;
				for (; sourceIndex <= toIndex; ++targetIndex, ++sourceIndex)
				{
					newValue[targetIndex].Value = newValue[sourceIndex].Value;
				}
			}
			else
			{
				var targetIndex = fromIndex;
				var sourceIndex = fromIndex - 1;
				for (; sourceIndex >= toIndex; --targetIndex, --sourceIndex)
				{
					newValue[targetIndex].Value = newValue[sourceIndex].Value;
				}
			}
			newValue[toIndex].Value = tempItem;
			this.Value = newValue;
		}

		protected override void LoadProperties()
		{
			this.LoadPropertiesCore<string>();
		}

		protected override WallpaperPathProperty CreateProperty(int index)
		{
			return new WallpaperPathProperty(this.CreateItemName(index), index, this.Provider);
		}

		protected override WallpaperPathProperty CreatePropertyWithDefault(int index, WallpaperPathProperty defaultValue)
		{
			return new WallpaperPathProperty(this.CreateItemName(index), index, this.Provider, defaultValue.Value);
		}

		protected override bool IsEmptyValue(WallpaperPathProperty value)
		{
			return value == null || string.IsNullOrEmpty(value.Value);
		}
	}


	public class WallpaperPositionsPropertyList : SerializablePropertyListBase<WallpaperPositionsProperty>
	{
		public WallpaperPositionsPropertyList(string key, ISerializationProvider provider) : base(key, provider) { }
		public WallpaperPositionsPropertyList(string key, int size, ISerializationProvider provider) : base(key, size, provider) { }
		public WallpaperPositionsPropertyList(string key, ISerializationProvider provider, params WallpaperPositionsProperty[] defaultValues) : base(key, provider, defaultValues) { }

		protected override void MoveCore(int fromIndex, int toIndex)
		{
			var newValue = this.Value.ToList();
			var tempItem = this.Value[fromIndex].Value;
			if (fromIndex < toIndex)
			{
				var targetIndex = fromIndex;
				var sourceIndex = fromIndex + 1;
				for (; sourceIndex <= toIndex; ++targetIndex, ++sourceIndex)
				{
					newValue[targetIndex].Value = newValue[sourceIndex].Value;
				}
			}
			else
			{
				var targetIndex = fromIndex;
				var sourceIndex = fromIndex - 1;
				for (; sourceIndex >= toIndex; --targetIndex, --sourceIndex)
				{
					newValue[targetIndex].Value = newValue[sourceIndex].Value;
				}
			}
			newValue[toIndex].Value = tempItem;
			this.Value = newValue;
		}

		protected override void LoadProperties()
		{
			this.LoadPropertiesCore<byte>();
		}

		protected override WallpaperPositionsProperty CreateProperty(int index)
		{
			return new WallpaperPositionsProperty(this.CreateItemName(index), index, this.Provider);
		}

		protected override WallpaperPositionsProperty CreatePropertyWithDefault(int index, WallpaperPositionsProperty defaultValue)
		{
			return new WallpaperPositionsProperty(this.CreateItemName(index), index, this.Provider, defaultValue.Value);
		}

		protected override bool IsEmptyValue(WallpaperPositionsProperty value)
		{
			return value == null;
		}
	}
}
