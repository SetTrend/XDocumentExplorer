namespace XDocumentExplorer.Code.FindReplace.Settings
{
	/// <summary>
	///		Represents the action to be processed
	///		when the Find/Replace dialog is dismissed.
	/// </summary>
	public enum FindReplaceMode
	{
		/// <summary>
		///		Find the next occurance of the search text.
		/// </summary>
		Find,

		/// <summary>
		///		Replace the next occurance of the search
		///		text with the replacement text.
		/// </summary>
		Replace,
	}
}
