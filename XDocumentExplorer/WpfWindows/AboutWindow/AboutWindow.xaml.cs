using System.Windows;
using System.Windows.Input;

using static XDocumentExplorer.Code.WindowExtensions;

namespace XDocumentExplorer.WpfWindows.AboutWindow
{
	/// <summary>
	/// Interaktionslogik für About.xaml
	/// </summary>
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
