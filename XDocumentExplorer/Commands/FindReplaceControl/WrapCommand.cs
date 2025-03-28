using System.Windows.Input;

namespace XDocumentExplorer.Commands.FindReplaceControl
{
	public class WrapCommand : RoutedCommand
	{
		public WrapCommand() => InputGestures.Add(new KeyGesture(Key.P, ModifierKeys.Control));
	}
}
