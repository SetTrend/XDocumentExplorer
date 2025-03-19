using System.Windows.Input;

namespace XDocumentExplorer.Commands.MainWindow
{
	public class FindNextCommand : RoutedCommand
	{
		public FindNextCommand() => InputGestures.Add(new KeyGesture(Key.F3));
	}
}
