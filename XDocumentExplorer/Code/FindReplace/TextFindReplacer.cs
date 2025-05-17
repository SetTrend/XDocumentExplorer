using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

using XDocumentExplorer.Code.FindReplace.Settings;
using XDocumentExplorer.WpfWindows.MainWindow;

using static XDocumentExplorer.Code.TextHelpers;

namespace XDocumentExplorer.Code.FindReplace
{
	internal class TextFindReplacer
	{
		private static readonly Regex _toRE = new Regex(@"[\W\\]", RegexOptions.Compiled | RegexOptions.NonBacktracking | RegexOptions.Singleline);
		private static readonly Regex _placeholderRE = new Regex(@"(?<!\\)\$\u0002(?'digit'[1-9])(?!\d)", RegexOptions.Compiled | RegexOptions.Singleline);

		private readonly TextBox _textBox;
		private readonly FindReplaceParams _settings;




		private Regex SearchExpression
		{
			get
			{
				string pattern = _settings.FindString;
				RegexOptions options = RegexOptions.None;

				if (!_settings.IsRegularExpression) pattern = _toRE.Replace(pattern, @"[$1]").Replace(@"\", @"\\");

				if (_settings.WholeWord)
				{
					if (!pattern.StartsWith(@"\b")) pattern = @"\b" + pattern;
					if (!pattern.EndsWith(@"\b")) pattern += @"\b";
				}

				if (!_settings.CaseSensitive) options |= RegexOptions.IgnoreCase;

				return new Regex(pattern, options, new TimeSpan(0, 0, 2));
			}
		}



		internal TextFindReplacer(TextBox textBox, FindReplaceParams settings) => (_textBox, _settings) = (textBox, settings);



		internal void FindNext()
		{
			(int offset, string text) = GetTextBoxTextSection(true, FindReplaceMode.Find);
			List<Match> results = FindAll(text);

			if (results.Count == 0 && (_settings.Wrap || MessageBox.Show(ReplaceNewLine(Messages.EndOfTextReached), ControlTexts.MainWin, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes))
			{
				(offset, text) = GetTextBoxTextSection(false, FindReplaceMode.Find);
				results = FindAll(text);
			}

			if (results.Count == 0) MessageBox.Show(ReplaceNewLine(Messages.TextNotFound), ControlTexts.MainWin, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
			else
			{
				Match result = results[0];

				_textBox.SelectionStart = offset + result.Index;
				_textBox.SelectionLength = result.Length;
			}
		}

		internal void FindPrevious()
		{
			(int offset, string text) = GetTextBoxTextSection(false, FindReplaceMode.Find);
			List<Match> results = FindAll(text);

			if (results.Count == 0 && (_settings.Wrap || MessageBox.Show(ReplaceNewLine(Messages.BeginningOfTextReached), ControlTexts.MainWin, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes))
			{
				(offset, text) = GetTextBoxTextSection(true, FindReplaceMode.Find);
				results = FindAll(text);
			}

			if (results.Count == 0) MessageBox.Show(ReplaceNewLine(Messages.TextNotFound), ControlTexts.MainWin, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
			else
			{
				Match result = results[^1];

				_textBox.SelectionStart = offset + result.Index;
				_textBox.SelectionLength = result.Length;
			}
		}

		internal void ReplaceNext()
		{
			(int offset, string text) = GetTextBoxTextSection(true, FindReplaceMode.Replace);
			List<Match> results = FindAll(text);

			if (results.Count == 0 && (_settings.Wrap || MessageBox.Show(ReplaceNewLine(Messages.EndOfTextReached), ControlTexts.MainWin, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes))
			{
				(offset, text) = GetTextBoxTextSection(false, FindReplaceMode.Replace);
				results = FindAll(text);
			}

			if (results.Count == 0) MessageBox.Show(ReplaceNewLine(Messages.TextNotFound), ControlTexts.MainWin, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
			else
			{
				Match result = results[0];
				int newStart = _textBox.SelectionStart;
				int newLen = _textBox.SelectionLength;
				int selEnd = newStart + newLen;

				(text, int lenDiff) = ReplaceResult(_textBox.Text, result, offset);

				int resultPos = offset + result.Index;

				if (selEnd > resultPos)
				{
					if (_textBox.SelectionStart > resultPos) newStart += lenDiff;
					if (newStart < 0) newStart = 0;
					else if (newStart > text.Length) newStart = text.Length;

					if (_textBox.SelectionStart < resultPos + result.Length && selEnd > resultPos) newLen += lenDiff;
					if (newLen < 0) newLen = 0;
					else if (newStart + newLen > text.Length) newLen = text.Length - newStart;
				}

				_textBox.Text = text;
				_textBox.SelectionStart = newStart;
				_textBox.SelectionLength = newLen;
				_textBox.InvalidateVisual();  // required here because the selection is not automatically updated in the text box if it doesn't have keyboard focus
			}
		}

		internal void ReplaceAll()
		{
			int offset = 0;
			string text = _textBox.Text;
			int newStart = _textBox.SelectionStart;
			int newLen = _textBox.SelectionLength;
			int selEnd = newStart + newLen;

			List<Match> results = FindAll(text);

			if (results.Count == 0) MessageBox.Show(ReplaceNewLine(Messages.TextNotFound), ControlTexts.MainWin, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
			else
			{
				foreach (Match result in results)
				{
					(text, int lenDiff) = ReplaceResult(text, result, offset);

					int resultPos = offset + result.Index;

					if (_textBox.SelectionStart > resultPos) newStart += lenDiff;
					if (newStart < 0) newStart = 0;
					else if (newStart > text.Length) newStart = text.Length;

					if (_textBox.SelectionStart < resultPos + result.Length && selEnd > resultPos) newLen += lenDiff;
					if (newLen < 0) newLen = 0;
					else if (newStart + newLen > text.Length) newLen = text.Length - newStart;

					offset += lenDiff;
				}

				_textBox.Text = text;
				_textBox.SelectionStart = newStart;
				_textBox.SelectionLength = newLen;
				_textBox.InvalidateVisual();  // required here because the selection is not automatically updated in the text box if it doesn't have keyboard focus
			}
		}



		private (int startIndex, string Text) GetTextBoxTextSection(bool forward, FindReplaceMode mode)
		{
			int forwardSearchStartOffset = forward
																			? _textBox.SelectionStart + (mode == FindReplaceMode.Replace || _textBox.SelectionLength == 0 ? 0 : 1)
																			: 0;

			string text = forward ? _textBox.Text[forwardSearchStartOffset..] : _textBox.Text[.._textBox.SelectionStart];

			return (forwardSearchStartOffset, text);
		}

		private List<Match> FindAll(string text)
		{
			List<Match> results = [];

			if (!(string.IsNullOrEmpty(_settings.FindString) || string.IsNullOrEmpty(text)))
				foreach (Match m in SearchExpression.Matches(text))
					results.Add(m);

			return results;
		}

		private (string NewString, int LenDiff) ReplaceResult(string text, Match result, int offset)
		{
			string replaceString = _settings.ReplaceString;
			StringBuilder sb = new StringBuilder(text.Length * 2);

			sb.Append(text[..(offset + result.Index)]);

			if (_settings.IsRegularExpression)
			{
				int lenDiff = 0;

				replaceString = replaceString.Replace("$", "$\u0002");

				foreach (Match m in _placeholderRE.Matches(replaceString))
				{
					Group replGrp = m.Groups["digit"];

					if (replGrp.Success && int.TryParse(replGrp.ToString(), out int digit) && result.Groups.Count >= digit)
					{
						string replacement = result.Groups[digit].ToString();
						int replStart = m.Index + lenDiff, replLen = m.Length;

						replaceString = replaceString[..replStart] + replacement + replaceString[(replStart + replLen)..];

						lenDiff += replacement.Length - replLen;
					}
				}

				replaceString = replaceString.Replace("$\u0002", "$");
			}

			sb
				.Append(replaceString)
				.Append(text[(offset + result.Index + result.Length)..])
				;

			return (sb.ToString(), replaceString.Length - result.Length);
		}
	}
}
