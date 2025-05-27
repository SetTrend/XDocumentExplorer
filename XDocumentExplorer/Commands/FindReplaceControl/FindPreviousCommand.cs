using System.Windows.Input;

namespace XDocumentExplorer.Commands.FindReplaceControl
{
	public class FindPreviousCommand : RoutedCommand
	{
		public FindPreviousCommand() => InputGestures.Add(new KeyGesture(Key.Enter, ModifierKeys.Shift));
	}
}
