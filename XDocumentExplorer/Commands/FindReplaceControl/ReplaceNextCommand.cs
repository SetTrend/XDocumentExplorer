using System.Windows.Input;

namespace XDocumentExplorer.Commands.FindReplaceControl
{
	public class ReplaceNextCommand : RoutedCommand
	{
		public ReplaceNextCommand() => InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));
	}
}
