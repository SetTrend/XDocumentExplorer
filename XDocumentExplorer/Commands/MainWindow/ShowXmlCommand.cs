using System.Windows.Input;

namespace XDocumentExplorer.Commands.MainWindow
{
	public class ShowXmlCommand : RoutedCommand
	{
		public ShowXmlCommand() => InputGestures.Add(new KeyGesture(Key.M, ModifierKeys.Control));
	}
}
