using System;

namespace XDocumentExplorer.Code
{
	public static class TextHelpers
	{
		/// <summary>
		///		Replaces the "<c>\n</c>" escape sequence
		///		in the specified string with a new-line
		///		character.
		/// </summary>
		/// <param name="text">
		///		Text to have each occurance of the
		///		"<c>\n</c>" escape sequence being replaced
		///		with a new-line character.
		/// </param>
		/// <returns>
		///		<see langword="string"/> with all occurances
		///		of the "<c>\n</c>" escape sequence being
		///		replaced with new-line characters.
		/// </returns>
		public static string ReplaceNewLine(string text) => text.Replace(@"\n", Environment.NewLine);
	}
}
