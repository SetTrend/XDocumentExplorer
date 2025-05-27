using System.Windows.Input;

namespace XDocumentExplorer.Commands.FindReplaceControl
{
	public class IsRegExCommand : RoutedCommand
	{
		public IsRegExCommand() => InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));
	}
}
