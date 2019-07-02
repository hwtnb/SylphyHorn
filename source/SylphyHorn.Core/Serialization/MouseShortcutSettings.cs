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
			ValidateCode(MoveLeftAndSwitch);
			ValidateCode(MoveRightAndSwitch);
			ValidateCode(MoveNewAndSwitch);
			ValidateCode(TogglePin);
		}

		private void ValidateCode(ShortcutkeyProperty property)
		{
			if (!property.Value.All(code => IsValidCode(code))) property.Value = null;
		}

		private bool IsValidCode(int code)
		{
			// (LButton <= code && code <= XButton2 && code != Cancel) || (WheelDown <= code && code < WheelUp)
			return (0 < code && code < 7 && code != 3) || (262144 * 2 < code && code < 262144 * 2 + 3);
		}
	}
}
