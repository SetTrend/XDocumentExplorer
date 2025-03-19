using System.Windows.Input;

namespace XDocumentExplorer.Commands.MainWindow
{
	public class LinqCommand : RoutedCommand
	{
		public LinqCommand() => InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Control));
	}
}
