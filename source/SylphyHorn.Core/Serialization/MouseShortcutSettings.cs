using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetroTrilithon.Serialization;

namespace SylphyHorn.Serialization
{
	public class MouseShortcutSettings : ShortcutKeySettings
	{
		public MouseShortcutSettings(ISerializationProvider provider)
			: base(provider)
		{
		}
	}
}
