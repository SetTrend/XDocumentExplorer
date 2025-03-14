using System.Windows.Input;

namespace XDocumentExplorer.Commands
{
	public class FocusCommand : RoutedCommand
	{
		public FocusCommand() => InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control));
	}
}
