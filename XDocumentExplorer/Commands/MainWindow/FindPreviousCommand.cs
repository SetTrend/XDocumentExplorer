using System.Windows.Input;

namespace XDocumentExplorer.Commands.MainWindow
{
	public class FindPreviousCommand : RoutedCommand
	{
		public FindPreviousCommand() => InputGestures.Add(new KeyGesture(Key.F3, ModifierKeys.Shift));
	}
}
