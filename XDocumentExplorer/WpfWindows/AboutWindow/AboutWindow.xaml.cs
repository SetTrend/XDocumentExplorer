using System.Windows;
using System.Windows.Input;

using static XDocumentExplorer.Code.TextHelpers;

namespace XDocumentExplorer.WpfWindows.AboutWindow
{
	public partial class AboutWindow : Window
	{
		public AboutWindow()
		{
			InitializeComponent();

			SetControlTexts();
		}



		private void SetControlTexts()
		{
			AboutWin.Title = ReplaceNewLine(ControlTexts.AboutWin);
			TitleText.Text = ReplaceNewLine(ControlTexts.TitleText);
			CopyrightText.Text = ReplaceNewLine(ControlTexts.CopyrightText);
			OkButton.Content = ReplaceNewLine(ControlTexts.OkButton);
		}



		private void OkButton_Executed(object sender, ExecutedRoutedEventArgs e) => Close();
	}
}
