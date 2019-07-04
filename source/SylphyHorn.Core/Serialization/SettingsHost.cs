using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SylphyHorn.Serialization
{
	public abstract class SettingsHost
	{
		private static readonly Dictionary<Type, SettingsHost> _instances = new Dictionary<Type, SettingsHost>();
		private readonly Dictionary<string, object> _cachedProperties = new Dictionary<string, object>();

		protected virtual string CategoryName => this.GetType().Name;

		protected SettingsHost()
		{
			_instances[this.GetType()] = this;
		}
		
		protected SerializableProperty<T> Cache<T>(Func<string, SerializableProperty<T>> create, [CallerMemberName] string propertyName = "")
		{
			var key = this.CategoryName + "." + propertyName;

			object obj;
			if (this._cachedProperties.TryGetValue(key, out obj) && obj is SerializableProperty<T>) return (SerializableProperty<T>)obj;

			var property = create(key);
			this._cachedProperties[key] = property;

			return property;
		}

		protected ShortcutkeyProperty Cache(Func<string, ShortcutkeyProperty> create, [CallerMemberName] string propertyName = "")
		{
			var key = this.CategoryName + "." + propertyName;

			object obj;
			if (this._cachedProperties.TryGetValue(key, out obj) && obj is ShortcutkeyProperty) return (ShortcutkeyProperty)obj;

			var property = create(key);
			this._cachedProperties[key] = property;

			return property;
		}

		protected ShortcutkeyPropertyList Cache(Func<string, ShortcutkeyPropertyList> create, [CallerMemberName] string propertyName = "")
		{
			var key = this.CategoryName + "." + propertyName;

			object obj;
			if (this._cachedProperties.TryGetValue(key, out obj) && obj is ShortcutkeyPropertyList) return (ShortcutkeyPropertyList)obj;

			var property = create(key);
			this._cachedProperties[key] = property;

			return property;
		}

		protected DesktopNamePropertyList Cache(Func<string, DesktopNamePropertyList> create, [CallerMemberName] string propertyName = "")
		{
			var key = this.CategoryName + "." + propertyName;

			object obj;
			if (this._cachedProperties.TryGetValue(key, out obj) && obj is DesktopNamePropertyList) return (DesktopNamePropertyList)obj;

			var property = create(key);
			this._cachedProperties[key] = property;

			return property;
		}

		public static T Instance<T>() where T : SettingsHost
		{
			SettingsHost host;
			return _instances.TryGetValue(typeof(T), out host) ? (T)host : null;
		}
	}
}
