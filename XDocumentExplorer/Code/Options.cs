using System;
using System.IO;
using System.Text.Json;
using System.Xml.Linq;

using XDocumentExplorer.Code.FindReplace.Settings;
using XDocumentExplorer.WpfWindows.MainWindow;

namespace XDocumentExplorer.Code
{
	/// <summary>
	///		Program settings, persisted in an
	///		application settings file.
	/// </summary>
	public class Options
	{
		private static readonly string _configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Axel Dahmen", ControlTexts.MainWin, "appConfig.json");
		private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions() { WriteIndented = true };

		private MruFileList _mruFiles = new MruFileList(2);
		private FindReplaceParams _findReplArgs = new FindReplaceParams();
		private XmlSaveOptions _xmlSaveOptions = new XmlSaveOptions();
		private byte _tabSize = 2;
		private bool _replaceTabs = true;
		private bool _enhancedWriter = false;



		/// <summary>
		///		Gets or sets a list of the most recently used files.
		/// </summary>
		public MruFileList MruFileList { get => _mruFiles; set => _mruFiles = value; }

		/// <summary>
		///		Gets or sets arguments for finding or replacing
		///		pieces of text.
		/// </summary>
		public FindReplaceParams FindReplaceSettings { get => _findReplArgs; set => _findReplArgs = value; }

		/// <summary>
		///		Gets or sets parameters determining how
		///		<see cref="XDocument"/> is serialized into
		///		an XML stream.
		/// </summary>
		public XmlSaveOptions XmlSaveOptions { get => _xmlSaveOptions; set => _xmlSaveOptions = value; }

		/// <summary>
		///		Gets or sets the number of space characters
		///		represented by a tab character.
		/// </summary>
		public byte TabSize
		{
			get => _tabSize;

			set
			{
				_tabSize = value switch
				{
					< 2 => 2,
					> 8 => 8,
					_ => value
				};
			}
		}

		/// <summary>
		///		<para>
		///			Gets or sets a value representing whether to replace
		///			tab characters with space characters.
		///		</para>
		///		<para>
		///			If <see langword="true"/>, replaces tab characters
		///			("<c>\t</c>") entered into the text input control
		///			with a corresponding number of space characters.
		///			When text is loaded from a file, any tab characters
		///			are replaced as well.
		///		</para>
		///		<para>
		///			If <see langword="false"/>, text in the input control
		///			is left unchanged.
		///		</para>
		/// </summary>
		/// <value>
		///		<see langword="true"/> for having all tab characters
		///		in the text input control being replaced with spaces;
		///		<see langword="false"/> otherwise.
		/// </value>
		public bool ReplaceTabs { get => _replaceTabs; set => _replaceTabs = value; }

		/// <summary>
		///		<para>
		///			Gets or sets a value representing whether the default
		///			Microsoft .NET <c>XmlWriter</c> or the <c>EnhancedXmlWriter</c>
		///			by Axel Dahmen will be used for the <see cref="XDocument"/>
		///			output panel.
		///		</para>
		///		<para>
		///			If <see langword="true"/>, the <c>EnhancedXmlWriter</c>
		///			by Axel Dahmen will be used; otherwise, the Microsoft
		///			.NET <c>XmlWriter</c> is used.
		///		</para>
		/// </summary>
		/// <value>
		///		<see langword="true"/> for using the <c>EnhancedXmlWriter</c>
		///			by Axel Dahmen; <see langword="false"/> for using the
		///			Microsoft .NET <c>XmlWriter</c>.
		/// </value>
		public bool EnhancedWriter { get => _enhancedWriter; set => _enhancedWriter = value; }



		/// <summary>
		///		Initializes a new <see cref="Options"/>
		///		object with default values.
		/// </summary>
		public Options() { }

		/// <summary>
		///		Initializes a new <see cref="Options"/>
		///		object by creating a shallow copy from
		///		the specified other <see cref="Options"/>
		///		object.
		/// </summary>
		/// <param name="other">
		///		<see cref="Options"/> object to copy
		///		values from.
		/// </param>
		public Options(Options other)
			=> (_mruFiles, _findReplArgs, _xmlSaveOptions, _tabSize, _replaceTabs, _enhancedWriter)
			= (new MruFileList(other._mruFiles), other._findReplArgs, other._xmlSaveOptions, other._tabSize, other._replaceTabs, other._enhancedWriter);



		/// <summary>
		///		Loads values for this <see cref="Options"/>
		///		object from configuration storage.
		/// </summary>
		public static Options Load()
		{
			if (File.Exists(_configFilePath))
			{
				using FileStream fs = new FileStream(_configFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

				return JsonSerializer.Deserialize<Options>(fs, _jsonOptions) ?? throw new InvalidOperationException("Cannot deserialize application configuration file. Invalid format.");
			}

			return new Options();
		}

		/// <summary>
		///		Saves this <see cref="Options"/> object's
		///		current values to configuration storage.
		/// </summary>
		public void Save()
		{
			if (!File.Exists(_configFilePath)) Directory.CreateDirectory(Path.GetDirectoryName(_configFilePath)!);

			using FileStream fs = new FileStream(_configFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

			JsonSerializer.Serialize(fs, this, _jsonOptions);
		}
	}
}
