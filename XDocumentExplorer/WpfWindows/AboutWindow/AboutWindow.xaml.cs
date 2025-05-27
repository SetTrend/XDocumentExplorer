using System;
using System.Reflection;
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
			Version version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0);

			AboutWin.Title = ReplaceNewLine(ControlTexts.AboutWin);
			TitleText.Text = ReplaceNewLine(ControlTexts.TitleText);
			CopyrightText.Text = ReplaceNewLine(ControlTexts.CopyrightText);
			VersionText.Text = ReplaceNewLine(string.Format(ControlTexts.VersionText, version.Major, version.Minor));
			OkButton.Content = ReplaceNewLine(ControlTexts.OkButton);
		}



		private void OkButton_Executed(object sender, ExecutedRoutedEventArgs e) => Close();
	}
}
