using System.Windows.Input;

namespace XDocumentExplorer.Commands.MainWindow
{
	public class UseLinqCommand : RoutedCommand
	{
		public UseLinqCommand() => InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Control));
	}
}
