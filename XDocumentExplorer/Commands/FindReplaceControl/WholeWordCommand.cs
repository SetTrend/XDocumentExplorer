using System.Windows.Input;

namespace XDocumentExplorer.Commands.FindReplaceControl
{
	public class WholeWordCommand : RoutedCommand
	{
		public WholeWordCommand() => InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control));
	}
}
