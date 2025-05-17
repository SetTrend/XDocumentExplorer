using System.Windows.Input;

namespace XDocumentExplorer.Commands.MainWindow
{
	public class QuitCommand : RoutedCommand
	{
		public QuitCommand() => InputGestures.Add(new KeyGesture(Key.F4, ModifierKeys.Alt));
	}
}
