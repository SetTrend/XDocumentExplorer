using System.Windows;
using System.Windows.Input;

using XDocumentExplorer.Code;

using static XDocumentExplorer.Code.TextHelpers;

namespace XDocumentExplorer.WpfWindows.OptionsWindow
{
	public partial class OptionsWindow : Window
	{
		public Options Options { get; init; }



		public OptionsWindow(Options options)
		{
			Options = new Options(options);

			InitializeComponent();

			SetControlTexts();
		}



		private void SetControlTexts()
		{
			OptionsWin.Title = ReplaceNewLine(ControlTexts.OptionsWin);

			MruCapacityLabel.Content = ReplaceNewLine(ControlTexts.MruFileCapacity);
			ReplaceTabsLabel.Content = ReplaceNewLine(ControlTexts.ReplaceTabs);
			TabSizeLabel.Content = ReplaceNewLine(ControlTexts.TabSize);
			EnhWriterLabel.Content = ReplaceNewLine(ControlTexts.EnhWriter);

			MruCapacityInfo.ToolTip = ReplaceNewLine(ControlTexts.MruCapacityInfo);
			ReplaceTabsInfo.ToolTip = ReplaceNewLine(ControlTexts.ReplaceTabsInfo);
			TabSizeInfo.ToolTip = ReplaceNewLine(ControlTexts.TabSizeInfo);
			EnhWriterInfo.ToolTip = ReplaceNewLine(ControlTexts.EnhWriterInfo);

			OkButton.Content = ReplaceNewLine(ControlTexts.OkButton);
			CancelButton.Content = ReplaceNewLine(ControlTexts.CancelButton);
		}



		private void Button_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			DialogResult = bool.TryParse(e.Parameter?.ToString(), out bool result) && result;

			Close();
		}
	}
}
