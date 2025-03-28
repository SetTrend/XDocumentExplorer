using System.Windows.Input;

namespace XDocumentExplorer.Commands.MainWindow.ShowXmlOutput;

internal class EscapeXmlCommand : RoutedCommand
{
	public EscapeXmlCommand() => InputGestures.Add(new KeyGesture(Key.F7));
}
