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


	public class ShortcutkeyProperty : SerializablePropertyBase<IList<int>>
	{
		private const string _emptyString = "(none)";

		public int Index { get; private set; } = 0;

		public string NumberText => (this.Index + 1).ToString();

		public ShortcutkeyProperty(string key, ISerializationProvider provider) : base(key, provider) { }
		public ShortcutkeyProperty(string key, ISerializationProvider provider, params int[] defaultValue) : base(key, provider, defaultValue) { }

		public ShortcutkeyProperty(string key, int index, ISerializationProvider provider) : base(key, provider)
		{
			this.Index = index;
		}

		public ShortcutkeyProperty(string key, int index, ISerializationProvider provider, params int[] defaultValue) : base(key, provider, defaultValue)
		{
			this.Index = index;
		}

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


	public class ShortcutkeyPropertyList : INotifyPropertyChanged
	{
		private IReadOnlyList<ShortcutkeyProperty> _value;

		public string Key { get; }

		public ISerializationProvider Provider { get; }

		public IReadOnlyList<ShortcutkeyProperty> Value
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

		public event PropertyChangedEventHandler PropertyChanged;

		public ShortcutkeyPropertyList(string key, int size, ISerializationProvider provider)
		{
			this.Key = key ?? throw new ArgumentNullException(nameof(key));
			this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));

			var value = new List<ShortcutkeyProperty>(size);
			for (var i = 0; i < size; ++i)
			{
				value.Add(new ShortcutkeyProperty(CreateItemName(i), i, provider));
			}
			this.Value = value;
		}

		public ShortcutkeyPropertyList(string key, ISerializationProvider provider, params ShortcutkeyProperty[] defaultValue)
		{
			if (defaultValue == null) defaultValue = Array.Empty<ShortcutkeyProperty>();

			this.Key = key ?? throw new ArgumentNullException(nameof(key));
			this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));

			var value = new List<ShortcutkeyProperty>(defaultValue.Length);
			for (var i = 0; i < defaultValue.Length; ++i)
			{
				value.Add(new ShortcutkeyProperty(CreateItemName(i), i, provider, defaultValue[i].Value.ToArray()));
			}
			this.Value = value;
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
				var newValue = oldValue.ToList();
				for (var i = oldValue.Count; i < size; ++i)
				{
					newValue.Add(new ShortcutkeyProperty(CreateItemName(i), i, this.Provider));
				}
				this.Value = newValue;
			}
		}

		private string CreateItemName(int index)
		{
			return $"{this.Key}[{index}]";
		}
	}
}
