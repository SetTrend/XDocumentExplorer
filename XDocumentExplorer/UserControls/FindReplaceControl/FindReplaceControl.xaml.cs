using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using XDocumentExplorer.Code.FindReplace;
using XDocumentExplorer.Code.FindReplace.Settings;
using XDocumentExplorer.Commands.FindReplaceControl;

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
																		, (DependencyObject d, DependencyPropertyChangedEventArgs e) => ((FindReplaceControl)d).SetTextFindReplacer()
																		)
			);

		private static readonly Dictionary<string, string> _modifierTexts = [];

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



		static FindReplaceControl()
		{
			foreach (PropertyInfo str in typeof(Messages).GetProperties(BindingFlags.Static | BindingFlags.NonPublic))
				if (str.GetValue(null) is string name && Enum.IsDefined(typeof(ModifierKeys), str.Name))
					_modifierTexts.Add(str.Name, name);
		}

		public FindReplaceControl()
		{
			InitializeComponent();

			SetControlTexts();
		}



		private void SetControlTexts()
		{
			SetButtonToolTip(CaseSensitiveButton, new CaseSensitivityCommand());
			SetButtonToolTip(WholeWordButton, new WholeWordCommand());
			SetButtonToolTip(IsRegExButton, new IsRegExCommand());
			SetButtonToolTip(WrapButton, new WrapCommand());

			SetButtonToolTip(FindNextButton, new FindNextCommand());
			SetButtonToolTip(FindPreviousButton, new FindPreviousCommand());
			SetButtonToolTip(ReplaceNextButton, new ReplaceNextCommand());
			SetButtonToolTip(ReplaceAllButton, new ReplaceAllCommand());

			FindLabel.Content = ControlTexts.FindLabel;
			ReplaceLabel.Content = ControlTexts.ReplaceLabel;

			FindText.ToolTip = ControlTexts.FindText;
			ReplaceText.ToolTip = ControlTexts.ReplaceText;
		}

		private static void SetButtonToolTip(ButtonBase button, RoutedCommand command)
		{
			if (typeof(ControlTexts).GetProperty(button.Name, BindingFlags.Static | BindingFlags.NonPublic) is PropertyInfo ctrlPi
				&& ctrlPi.GetValue(null) is string toolTip)
			{
				foreach (InputGesture gesture in command.InputGestures)
					if (gesture is KeyGesture keyGesture)
					{
						StringBuilder sb = new StringBuilder(128);

						sb
							.Append(toolTip)
							.Append("  (");
						;

						if (keyGesture.Modifiers != ModifierKeys.None)
							foreach (ModifierKeys value in Enum.GetValues<ModifierKeys>())
								if (value != ModifierKeys.None && (keyGesture.Modifiers & value) == value && Enum.GetName(value) is string modifier)
								{
									if (_modifierTexts.TryGetValue(modifier, out string? newName)) modifier = newName;

									sb
										.Append(modifier)
										.Append('+')
										;

									break;
								}

						sb
							.Append(keyGesture.Key.ToString())
							.Append(')')
							;

						toolTip = sb.ToString();

						break;
					}

				button.ToolTip = toolTip;
			}
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
