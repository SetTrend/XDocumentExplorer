using System.Windows.Input;

namespace XDocumentExplorer.Commands.MainWindow
{
	public class QuickFindPreviousCommand : RoutedCommand
	{
		public QuickFindPreviousCommand() => InputGestures.Add(new KeyGesture(Key.F3, ModifierKeys.Control | ModifierKeys.Shift));
	}
}
