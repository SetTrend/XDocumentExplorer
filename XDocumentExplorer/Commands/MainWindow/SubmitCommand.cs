using System.Windows.Input;

namespace XDocumentExplorer.Commands.MainWindow
{
	public class SubmitCommand : RoutedCommand
	{
		public SubmitCommand() => InputGestures.Add(new KeyGesture(Key.U, ModifierKeys.Control));
	}
}
