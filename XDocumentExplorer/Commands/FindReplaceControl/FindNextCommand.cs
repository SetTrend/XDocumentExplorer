using System.Windows.Input;

namespace XDocumentExplorer.Commands.FindReplaceControl
{
	public class FindNextCommand : RoutedCommand
	{
		public FindNextCommand() => InputGestures.Add(new KeyGesture(Key.Enter));
	}
}
