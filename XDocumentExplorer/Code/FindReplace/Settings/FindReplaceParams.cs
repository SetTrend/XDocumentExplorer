using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace XDocumentExplorer.Code.FindReplace.Settings
{
	/// <summary>
	///		Represents Find/Replace dialog input values.
	/// </summary>
	public class FindReplaceParams : INotifyPropertyChanged
	{
		private const int _maxStringLen = 256;
		private string _findString = "", _replaceString = "";
		private bool _wholeWord, _caseSensitive = true, _isRegEx, _wrap;

		public event PropertyChangedEventHandler? PropertyChanged;


		/// <summary>
		///		Gets or sets the text to be found.
		/// </summary>
		[JsonIgnore]
		public string FindString
		{
			get => _findString;

			set
			{
				if (value == null) _findString = "";
				else if (value.Length > _maxStringLen) throw new ArgumentOutOfRangeException(nameof(FindString), $"Find text exceeds the maximum allowed number of {_maxStringLen} characters.");
				else _findString = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FindString)));
			}
		}

		/// <summary>
		///		Gets or sets the replacement text.
		/// </summary>
		[JsonIgnore]
		public string ReplaceString
		{
			get => _replaceString;

			set
			{
				if (value == null) _replaceString = "";
				else if (value.Length > _maxStringLen) throw new ArgumentOutOfRangeException(nameof(ReplaceString), $"Find text exceeds the maximum allowed number of {_maxStringLen} characters.");
				else _replaceString = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReplaceString)));
			}
		}



		/// <summary>
		///		Gets or sets a value specifying whether
		///		the search text represents a whole word.
		/// </summary>
		/// <value>
		///		<see langword="true"/>, if the search text
		///		represents a whole word; <see langword="false"/>,
		///		if the search text may occur anywhere in
		///		the text about to be examined.
		/// </value>
		public bool WholeWord
		{
			get => _wholeWord;

			set
			{
				_wholeWord = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WholeWord)));
			}
		}

		/// <summary>
		///		Gets or sets a value specifying whether
		///		the text search will be performed case
		///		sensitive.
		/// </summary>
		/// <value>
		///		<see langword="true"/>, if the search
		///		will be performed case sensitive;
		///		<see langword="false"/>, if the search
		///		will be performed case insensitive.
		/// </value>
		public bool CaseSensitive
		{
			get => _caseSensitive;

			set
			{
				_caseSensitive = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CaseSensitive)));
			}
		}

		/// <summary>
		///		Gets or sets a value specifying
		///		whether the search text represents
		///		a <see cref="Regex"/> expression.
		/// </summary>
		/// <value>
		///		<see langword="true"/>, if the
		///		search text represents a regular
		///		expression; <see langword="false"/>,
		///		if search considers the input as
		///		plain text.
		/// </value>
		public bool IsRegularExpression
		{
			get => _isRegEx;

			set
			{
				_isRegEx = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRegularExpression)));
			}
		}

		/// <summary>
		///		Gets or sets a value specifying whether
		///		the next search starts from the beginning
		///		of the text when no more matches could be
		///		found from the current selection to the
		///		end of the text.
		/// </summary>
		/// <value>
		///		<see langword="true"/>, if the next search
		///		starts from the beginning of the text when
		///		no more matches could be found from the
		///		current selection to the end of the text;
		///		<see langword="false"/>, if the search
		///		stops at the end of the text.
		/// </value>
		public bool Wrap
		{
			get => _wrap;

			set
			{
				_wrap = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Wrap)));
			}
		}



		/// <summary>
		///		Initializes a new <see cref="FindReplaceParams"/>,
		///		empty object.
		/// </summary>
		public FindReplaceParams() { }
	}
}
