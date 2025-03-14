using System;
using System.IO;
using System.Text.Json;

using XDocumentExplorer.WpfWindows.MainWindow;

namespace XDocumentExplorer.Code
{
	public class Options
	{
		private static readonly string _configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Axel Dahmen", ControlTexts.MainWin, "appConfig.json");
		private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions() { WriteIndented = true };

		private MruFileList _mruFiles = new MruFileList(2);
		private byte _tabSize = 2;
		private bool _replaceTabs = true;



		/// <summary>
		///		Gets a list of the most recently used files.
		/// </summary>
		public MruFileList MruFileList { get => _mruFiles; set => _mruFiles = value; }

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
		public Options(Options other) => (_mruFiles, _tabSize, _replaceTabs) = (new MruFileList(other._mruFiles), other._tabSize, other._replaceTabs);



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
