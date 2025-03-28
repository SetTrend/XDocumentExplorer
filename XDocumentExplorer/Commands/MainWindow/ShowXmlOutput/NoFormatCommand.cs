using System.Windows.Input;

namespace XDocumentExplorer.Commands.MainWindow.ShowXmlOutput;

internal class NoFormatCommand : RoutedCommand
{
	public NoFormatCommand() => InputGestures.Add(new KeyGesture(Key.F5));
}
