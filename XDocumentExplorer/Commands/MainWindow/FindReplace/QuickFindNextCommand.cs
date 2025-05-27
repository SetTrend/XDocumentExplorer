using System.Windows.Input;

namespace XDocumentExplorer.Commands.MainWindow.FindReplace
{
	public class QuickFindNextCommand : RoutedCommand
	{
		public QuickFindNextCommand() => InputGestures.Add(new KeyGesture(Key.F3, ModifierKeys.Control));
	}
}
