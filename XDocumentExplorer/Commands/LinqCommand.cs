using System.Windows.Input;

namespace XDocumentExplorer.Commands
{
	public class LinqCommand : RoutedCommand
	{
		public LinqCommand() => InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Control));
	}
}
