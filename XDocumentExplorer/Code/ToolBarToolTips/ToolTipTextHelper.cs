using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using static XDocumentExplorer.Code.TextHelpers;

namespace XDocumentExplorer.Code.ToolBarToolTips;

internal static class ToolTipTextHelper
{
	private static readonly Dictionary<string, string> _modifierTexts = [];



	static ToolTipTextHelper()
	{
		foreach (PropertyInfo str in typeof(Modifiers).GetProperties(BindingFlags.Static | BindingFlags.NonPublic))
			if (str.GetValue(null) is string name && Enum.IsDefined(typeof(ModifierKeys), str.Name))
				_modifierTexts.Add(str.Name, name);
	}



	internal static void SetButtonToolTip(Type textClass, ButtonBase button, RoutedCommand command)
	{
		if (textClass.GetProperty(button.Name, BindingFlags.Static | BindingFlags.NonPublic) is PropertyInfo ctrlPi
			&& ctrlPi.GetValue(null) is string toolTip)
		{
			foreach (InputGesture gesture in command.InputGestures)
				if (gesture is KeyGesture keyGesture)
				{
					StringBuilder sb = new StringBuilder(128);

					sb
						.Append(toolTip)
						.Append("  (");
					;

					if (keyGesture.Modifiers != ModifierKeys.None)
						foreach (ModifierKeys value in Enum.GetValues<ModifierKeys>())
							if (value != ModifierKeys.None && (keyGesture.Modifiers & value) == value && Enum.GetName(value) is string modifier)
							{
								if (_modifierTexts.TryGetValue(modifier, out string? newName)) modifier = newName;

								sb
									.Append(modifier)
									.Append('+')
									;

								break;
							}

					sb
						.Append(keyGesture.Key.ToString())
						.Append(')')
						;

					toolTip = ReplaceNewLine(sb.ToString());

					break;
				}

			button.ToolTip = toolTip;
		}
	}
}
