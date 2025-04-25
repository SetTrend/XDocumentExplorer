using System.ComponentModel;
using System.Xml.Linq;

namespace XDocumentExplorer.Code;

/// <summary>
///		Parameters for serializing an <see cref="XDocument"/>
///		into an XML stream.
/// </summary>
public class XmlSaveOptions : INotifyPropertyChanged
{
	private bool _escapeXml, _disableFormatting, _omitDuplicateNamespaces;



	public event PropertyChangedEventHandler? PropertyChanged;



	/// <summary>
	///		Escape all non-printable characters.
	/// </summary>
	public bool EscapeXml
	{
		get => _escapeXml;

		set
		{
			_escapeXml = value;

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EscapeXml)));
		}
	}

	/// <summary>
	///		Preserve all insignificant white space while serializing.
	/// </summary>
	public bool DisableFormatting
	{
		get => _disableFormatting;

		set
		{
			_disableFormatting = value;

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisableFormatting)));
		}
	}

	/// <summary>
	///		Remove the duplicate namespace declarations while serializing.
	/// </summary>
	public bool OmitDuplicateNamespaces
	{
		get => _omitDuplicateNamespaces;

		set
		{
			_omitDuplicateNamespaces = value;

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OmitDuplicateNamespaces)));
		}
	}
}
