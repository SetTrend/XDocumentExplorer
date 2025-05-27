using System.Windows.Input;

namespace XDocumentExplorer.Commands.FindReplaceControl
{
	public class ReplaceAllCommand : RoutedCommand
	{
		public ReplaceAllCommand() => InputGestures.Add(new KeyGesture(Key.L, ModifierKeys.Control));
	}
}
