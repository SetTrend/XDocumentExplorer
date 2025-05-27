using System.Windows.Input;

namespace XDocumentExplorer.Commands.MainWindow.ShowXmlOutput;

internal class NoDuplicatesCommand : RoutedCommand
{
	public NoDuplicatesCommand() => InputGestures.Add(new KeyGesture(Key.F6));
}
