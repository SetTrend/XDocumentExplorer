using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using XDocumentExplorer.Code.FindReplace;
using XDocumentExplorer.Code.FindReplace.Settings;
using XDocumentExplorer.Commands.FindReplaceControl;

using static XDocumentExplorer.Code.ToolBarToolTips.ToolTipTextHelper;

namespace XDocumentExplorer.UserControls.FindReplaceControl
{
	public partial class FindReplaceControl : UserControl
	{
		public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(
				"Settings"
			, typeof(FindReplaceParams)
			, typeof(FindReplaceControl)
			, new FrameworkPropertyMetadata(new FindReplaceParams()
																		, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
																		, (depObj, propChangedEventArgs) => ((FindReplaceControl)depObj).SetTextFindReplacer()
																		)
			);

		private TextBox? _textBox;
		private TextFindReplacer? _replacer;
		private FindReplaceMode _mode = FindReplaceMode.Replace;



		public FindReplaceParams Settings
		{
			get => (FindReplaceParams)GetValue(SettingsProperty);

			set => SetValue(SettingsProperty, value);
		}

		public TextBox? TextBox
		{
			get => _textBox;

			set
			{
				_textBox = value;

				SetTextFindReplacer();
			}
		}


		public FindReplaceMode Mode
		{
			get => _mode;

			set
			{
				_mode = value;

				bool showAll = Mode == FindReplaceMode.Replace;

				ReplaceLabel.Visibility = ReplaceText.Visibility = ReplaceNextButton.Visibility = ReplaceAllButton.Visibility = showAll ? Visibility.Visible : Visibility.Collapsed;
				FindText.Focus();
			}
		}



		public FindReplaceControl()
		{
			InitializeComponent();

			SetControlTexts();
		}



		private void SetControlTexts()
		{
			Type ctrlTexts = typeof(ControlTexts);

			SetButtonToolTip(ctrlTexts, CaseSensitiveButton, new CaseSensitivityCommand());
			SetButtonToolTip(ctrlTexts, WholeWordButton, new WholeWordCommand());
			SetButtonToolTip(ctrlTexts, IsRegExButton, new IsRegExCommand());
			SetButtonToolTip(ctrlTexts, WrapButton, new WrapCommand());

			SetButtonToolTip(ctrlTexts, FindNextButton, new FindNextCommand());
			SetButtonToolTip(ctrlTexts, FindPreviousButton, new FindPreviousCommand());
			SetButtonToolTip(ctrlTexts, ReplaceNextButton, new ReplaceNextCommand());
			SetButtonToolTip(ctrlTexts, ReplaceAllButton, new ReplaceAllCommand());

			FindLabel.Content = ControlTexts.FindLabel;
			ReplaceLabel.Content = ControlTexts.ReplaceLabel;

			FindText.ToolTip = ControlTexts.FindText;
			ReplaceText.ToolTip = ControlTexts.ReplaceText;
		}

		private void SetTextFindReplacer() => _replacer = TextBox == null ? null : new TextFindReplacer(TextBox, Settings);



		private void CaseSensitivity_Executed(object sender, ExecutedRoutedEventArgs e) => Settings.CaseSensitive = !Settings.CaseSensitive;

		private void WholeWord_Executed(object sender, ExecutedRoutedEventArgs e) => Settings.WholeWord = !Settings.WholeWord;

		private void IsRegularExpression_Executed(object sender, ExecutedRoutedEventArgs e) => Settings.IsRegularExpression = !Settings.IsRegularExpression;

		private void Wrap_Executed(object sender, ExecutedRoutedEventArgs e) => Settings.Wrap = !Settings.Wrap;



		private void Find_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = _replacer != null && FindText.Text.Length > 0 && TextBox?.Text.Length > 0;

		private void FindNext_Executed(object sender, ExecutedRoutedEventArgs e) => _replacer?.FindNext();

		private void FindPrev_Executed(object sender, ExecutedRoutedEventArgs e) => _replacer?.FindPrevious();

		private void Replace_CanExecute(object sender, CanExecuteRoutedEventArgs e) => Find_CanExecute(sender, e);

		private void ReplaceNext_Executed(object sender, ExecutedRoutedEventArgs e) => _replacer?.ReplaceNext();

		private void ReplaceAll_Executed(object sender, ExecutedRoutedEventArgs e) => _replacer?.ReplaceAll();
	}
}
