using System.Windows.Input;

namespace XDocumentExplorer.Commands.MainWindow.FindReplace
{
	public class FindNextCommand : RoutedCommand
	{
		public FindNextCommand() => InputGestures.Add(new KeyGesture(Key.F3));
	}
}
