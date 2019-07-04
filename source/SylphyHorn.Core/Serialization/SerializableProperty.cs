using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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


	public abstract class SerializablePropertyListBase<T> : INotifyPropertyChanged
	{
		private IReadOnlyList<T> _value;

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

				var isValueChanged = this._value?.Count != value?.Count;
				this._value = value;

				if (isValueChanged) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Value)));
			}
		}

		public int Count => this._value.Count;

		public event PropertyChangedEventHandler PropertyChanged;

		public SerializablePropertyListBase(string key, int size, ISerializationProvider provider)
		{
			this.Key = key ?? throw new ArgumentNullException(nameof(key));
			this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));

			AddNewProperties(size);
		}

		public SerializablePropertyListBase(string key, ISerializationProvider provider, params T[] defaultValues)
		{
			if (defaultValues == null) defaultValues = Array.Empty<T>();

			this.Key = key ?? throw new ArgumentNullException(nameof(key));
			this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));

			FillNewPropertiesWithDefaultValues(defaultValues);
		}

		public void Resize(int size)
		{
			var oldValue = this.Value;
			if (oldValue.Count > size)
			{
				this.Value = oldValue.ToList().GetRange(0, size);
			}
			else if (oldValue.Count < size)
			{
				AddNewProperties(size);
			}
		}

		protected abstract void AddNewProperties(int newSize);
		protected abstract void FillNewPropertiesWithDefaultValues(T[] defaultValues);

		protected string CreateItemName(int index)
		{
			return $"{this.Key}[{index}]";
		}
	}


	public class ShortcutkeyPropertyList : SerializablePropertyListBase<ShortcutkeyProperty>
	{
		public ShortcutkeyPropertyList(string key, int size, ISerializationProvider provider) : base(key, size, provider) { }
		public ShortcutkeyPropertyList(string key, ISerializationProvider provider, params ShortcutkeyProperty[] defaultValues) : base(key, provider, defaultValues) { }

		protected override void AddNewProperties(int newSize)
		{
			var oldValue = this.Value ?? new List<ShortcutkeyProperty>(newSize);
			var newValue = oldValue.ToList();
			for (var i = oldValue.Count; i < newSize; ++i)
			{
				newValue.Add(new ShortcutkeyProperty(CreateItemName(i), i, this.Provider));
			}
			this.Value = newValue;
		}

		protected override void FillNewPropertiesWithDefaultValues(ShortcutkeyProperty[] defaultValues)
		{
			var value = new List<ShortcutkeyProperty>(defaultValues.Length);
			for (var i = 0; i < defaultValues.Length; ++i)
			{
				value.Add(new ShortcutkeyProperty(CreateItemName(i), i, this.Provider, defaultValues[i].Value.ToArray()));
			}
			this.Value = value;
		}
	}


	public class DesktopNamePropertyList : SerializablePropertyListBase<DesktopNameProperty>
	{
		public DesktopNamePropertyList(string key, int size, ISerializationProvider provider) : base(key, size, provider) { }

		public DesktopNamePropertyList(string key, ISerializationProvider provider, params DesktopNameProperty[] defaultValues) : base(key, provider, defaultValues) { }

		protected override void AddNewProperties(int newSize)
		{
			var oldValue = this.Value ?? new List<DesktopNameProperty>(newSize);
			var newValue = oldValue.ToList();
			for (var i = oldValue.Count; i < newSize; ++i)
			{
				newValue.Add(new DesktopNameProperty(CreateItemName(i), i, this.Provider));
			}
			this.Value = newValue;
		}

		protected override void FillNewPropertiesWithDefaultValues(DesktopNameProperty[] defaultValues)
		{
			var value = new List<DesktopNameProperty>(defaultValues.Length);
			for (var i = 0; i < defaultValues.Length; ++i)
			{
				value.Add(new DesktopNameProperty(CreateItemName(i), i, this.Provider, defaultValues[i].Value));
			}
			this.Value = value;
		}
	}
}
