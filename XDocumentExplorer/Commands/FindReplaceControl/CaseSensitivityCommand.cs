using System.Windows.Input;

namespace XDocumentExplorer.Commands.FindReplaceControl
{
	public class CaseSensitivityCommand : RoutedCommand
	{
		public CaseSensitivityCommand() => InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
	}
}
