using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Win32;

using XDocumentExplorer.Code;
using XDocumentExplorer.Code.FindReplace;
using XDocumentExplorer.Code.FindReplace.Settings;
using XDocumentExplorer.Commands.MainWindow;
using XDocumentExplorer.Commands.MainWindow.ShowXmlOutput;

using static XDocumentExplorer.Code.TextHelpers;
using static XDocumentExplorer.Code.ToolBarToolTips.ToolTipTextHelper;

using A = XDocumentExplorer.WpfWindows.AboutWindow;
using O = XDocumentExplorer.WpfWindows.OptionsWindow;
using W = AxDa.EnhancedXmlWriter;

namespace XDocumentExplorer.WpfWindows.MainWindow;

public partial class MainWindow : Window
{
	private const string _newLineEscape = "\u0085";

	private static readonly (string Key, string Value)[] _constantChars =
	[
		(@"\", @"\\"),
		("\0", @"\0"),
		("\a", @"\a"),
		("\b", @"\b"),
		("\e", @"\e"),
		("\f", @"\f"),
		("\n", @"\n" + _newLineEscape),
		("\r", @"\r"),
		("\t", @"\t"),
		("\v", @"\v")
	];

	private static readonly string[] _dragDropFileFormats =
	[
		DataFormats.FileDrop,
		"FileNameW",
		"FileName"
	];

	private const string _treeIconPath = @"pack://application:,,,/TreeIcons/";

	private Options _options = Options.Load();

	private static readonly Regex _codeRE = new Regex(@"\s*new\s+XDocument\(\s*", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);
	private static readonly Regex _trimCodeRE = new Regex(@"\A\s*(?'content'.*?);*\s*\Z", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
	private static readonly Regex _mruRE = new Regex(@"^(?'begin'(?:[\\/]{2})?[^\\/]+[\\/])(?'middle'.+)(?'trailing'(?:[\\/][^\\/]+){3,})$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
	private static readonly Regex _tabRE = new Regex(@"^(?'prefix'[^\t\r\n]*)\t", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.NonBacktracking);
	private static readonly Regex _wordCharsRE = new Regex(@"^\w+", RegexOptions.Compiled | RegexOptions.NonBacktracking);

	private static readonly DependencyProperty TitleTextProperty = DependencyProperty.Register("TitleText"
																																														, typeof(string)
																																														, typeof(Window)
																																														, new PropertyMetadata(ControlTexts.MainWin)
																																														);

	private static readonly DependencyProperty UseCodeProperty = DependencyProperty.Register("UseCode"
																																													, typeof(bool)
																																													, typeof(Window)
																																													, new FrameworkPropertyMetadata(false
																																																													, FrameworkPropertyMetadataOptions.AffectsMeasure
																																																													, new PropertyChangedCallback(OnUseCodeChanged)
																																																													)
																																													);

	private static readonly DependencyProperty ShowXmlProperty = DependencyProperty.Register("ShowXml"
																																													, typeof(bool)
																																													, typeof(Window)
																																													, new FrameworkPropertyMetadata(false
																																																													, FrameworkPropertyMetadataOptions.AffectsMeasure
																																																													, new PropertyChangedCallback(OnShowXmlChanged)
																																																													)
																																													);

	private bool _showBlanks;
	private bool _applyCodeChange = true, _applyTextChange = true;
	private string? _filePath;
	private bool _isDirty;
	private bool _draggingFindDlg;
	private double _findMouseX, _findWidth;



	public string TitleText
	{
		get => (string)GetValue(TitleTextProperty);
		set => SetValue(TitleTextProperty, value);
	}

	public bool UseCode
	{
		get => (bool)GetValue(UseCodeProperty);
		set => SetValue(UseCodeProperty, value);
	}

	public bool ShowBlanks
	{
		get => _showBlanks;

		set
		{
			_showBlanks = value;

			UpdateTreeTexts();
			WriteLinqToXml();
		}
	}

	public bool ShowXml
	{
		get => (bool)GetValue(ShowXmlProperty);
		set => SetValue(ShowXmlProperty, value);
	}


	public Options Options => _options;



	public MainWindow()
	{
		InitializeComponent();

		SetWindowTitleAndDirtyFlag();
		SetControlTexts();
		SetMenuTexts();

		SetMruFileMenu();
	}



	private void SetWindowTitleAndDirtyFlag(bool isDirty = false)
	{
		StringBuilder sb = new StringBuilder(256);

		sb.Append(ControlTexts.MainWin);

		if (!string.IsNullOrWhiteSpace(_filePath))
			sb
				.Append(" - ")
				.Append(Path.GetFileName(_filePath))
				;

		if (isDirty) sb.Append('*');

		_isDirty = isDirty;
		TitleText = sb.ToString();
	}

	private void SetFindControlWidth(double newWidth)
	{
		const double minMarginWidth = 10;

		FindReplaceBox.Width = newWidth >= minMarginWidth
			? newWidth < InputXml.ActualWidth - minMarginWidth
				? newWidth
				: InputXml.ActualWidth - minMarginWidth
			: minMarginWidth;
	}

	private void SetControlTexts()
	{
		Type ctrlTexts = typeof(ControlTexts);

		SubmitButton.Content = ControlTexts.SubmitButton;

		SetButtonToolTip(ctrlTexts, NewButton, ApplicationCommands.New);
		SetButtonToolTip(ctrlTexts, OpenButton, ApplicationCommands.Open);
		SetButtonToolTip(ctrlTexts, SaveButton, ApplicationCommands.Save);

		SetButtonToolTip(ctrlTexts, CutButton, ApplicationCommands.Cut);
		SetButtonToolTip(ctrlTexts, CopyButton, ApplicationCommands.Copy);
		SetButtonToolTip(ctrlTexts, PasteButton, ApplicationCommands.Paste);

		SetButtonToolTip(ctrlTexts, FindButton, ApplicationCommands.Find);
		SetButtonToolTip(ctrlTexts, ReplaceButton, ApplicationCommands.Replace);

		SetButtonToolTip(ctrlTexts, ShowXmlButton, new ShowXmlCommand());

		SetButtonToolTip(ctrlTexts, NoFormatButton, new NoFormatCommand());
		SetButtonToolTip(ctrlTexts, OmitDuplicateNsButton, new NoDuplicatesCommand());
		SetButtonToolTip(ctrlTexts, EscapeXmlButton, new EscapeXmlCommand());
	}

	private void SetMenuTexts()
	{
		ResourceManager resMan = new ResourceManager("XDocumentExplorer.WpfWindows.MainWindow.MenuItems", typeof(MainWindow).Assembly);

		foreach (Control child in MainMenu.Items) if (child is MenuItem menuItem) SetMenuItemText(menuItem, resMan);
	}

	private static void SetMenuItemText(MenuItem item, ResourceManager resMan)
	{
		foreach (Control child in item.Items) if (child is MenuItem menuItem) SetMenuItemText(menuItem, resMan);

		if (resMan.GetString(item.Name) is string text) item.Header = text;
	}



	private void AddCurrentFilePath(string filePath)
	{
		if (!string.IsNullOrWhiteSpace(filePath))
		{
			_options.MruFileList.Add(filePath);
			_filePath = filePath;

			SetMruFileMenu();
			SetWindowTitleAndDirtyFlag();
		}
	}

	private void SetMruFileMenu()
	{
		IReadOnlyList<string> mruFiles = _options.MruFileList.MruFiles;

		if (mruFiles.Count == 0) FileMenuRecentSep.Visibility = FileMenuRecent.Visibility = Visibility.Collapsed;
		else
		{
			FileMenuRecentSep.Visibility = FileMenuRecent.Visibility = Visibility.Visible;

			foreach (MenuItem item in FileMenuRecent.Items) item.Click -= FileMru_Click;
			FileMenuRecent.Items.Clear();

			for (byte i = 0;i < mruFiles.Count;++i)
			{
				string header = (1 + i).ToString();
				string name = mruFiles[i];

				Match m = _mruRE.Match(name);

				if (m.Success && m.Groups["middle"].Length > 2) name = m.Groups["begin"].ToString() + '…' + m.Groups["trailing"];

				MenuItem item = new MenuItem
				{
					Header = header[..(header.Length - 1)] + '_' + header[^1] + ' ' + name,
					Tag = mruFiles[i]
				};

				item.Click += FileMru_Click;

				FileMenuRecent.Items.Add(item);
			}
		}
	}



	private void ResetControls(bool useCode)
	{
		_filePath = null;
		ResetControls(useCode ? "new XDocument()" : "");
	}

	private void ResetControls(string initialContent)
	{
		_applyTextChange = false;
		InputXml.Text = initialContent;
		_applyTextChange = true;

		ErrorList.Text = "";
		WriteLinqToXml(null);

		XmlNodesTree.Items.Clear();

		SetWindowTitleAndDirtyFlag();
	}



	private void CreateTreeFromTextInput()
	{
		ErrorList.Text = "";
		WriteLinqToXml(null);

		XmlNodesTree.Items.Clear();

		try
		{
			XDocument xDoc = new XDocument();

			Mouse.OverrideCursor = Cursors.Wait;

			if (UseCode)
			{
				Task<object> t = CSharpScript.EvaluateAsync<object>(_trimCodeRE.Match(InputXml.Text).Groups["content"].ToString()
																														, ScriptOptions
																															.Default
																															.WithImports("System", "System.Xml", "System.Xml.Linq")
																															.WithReferences("netstandard.dll")
																														);

				t.Wait(2_000);

				if (t.IsCompletedSuccessfully && t.Result is XDocument xd) xDoc = xd;
			}
			else xDoc = XDocument.Parse(InputXml.Text);

			CreateTreeFromXDocument(xDoc);
		}
		catch (Exception ex)
		{
			ErrorList.Text = ex.Message;

			if (ex.InnerException != null) ErrorList.Text += Environment.NewLine + Environment.NewLine + ex.InnerException.Message;
		}
		finally
		{
			Mouse.OverrideCursor = null;
		}
	}

	private void CreateTreeFromXDocument(XDocument xDoc)
	{
		ErrorList.Text = "";
		WriteLinqToXml(null);

		XmlNodesTree.Items.Clear();

		ItemCollection target = XmlNodesTree.Items;

		target = CreateXmlPITreeItem(xDoc, target);
		CreateDocumentTypeTreeItem(xDoc, target);

		try
		{
			if (xDoc.Root != null) CreateElementTreeItem(xDoc.Root, target);

			WriteLinqToXml(xDoc);
		}
		catch (Exception ex)
		{
			ErrorList.Text = ex.Message;

			if (ex.InnerException != null) ErrorList.Text += Environment.NewLine + Environment.NewLine + ex.InnerException.Message;
		}
	}

	private ItemCollection CreateXmlPITreeItem(XDocument xDoc, ItemCollection target)
	{
		if (xDoc.Declaration != null)
		{
			XDeclaration xDecl = xDoc.Declaration;
			StringBuilder sb = new StringBuilder(256);

			if (xDecl.Version != null)
				sb
					.Append(nameof(XDeclaration.Version))
					.Append(" = ")
					.Append(xDecl.Version)
					.Append(", ")
					;

			if (xDecl.Encoding != null)
				sb
					.Append(nameof(XDeclaration.Encoding))
					.Append(" = ")
					.Append(xDecl.Encoding)
					.Append(", ")
					;

			if (xDecl.Standalone != null)
				sb
					.Append(nameof(XDeclaration.Standalone))
					.Append(" = ")
					.Append(xDecl.Standalone)
					.Append(", ")
					;

			if (sb.Length > 2) sb.Length -= 2;

			return CreateTreeItem(target, typeof(XDeclaration), "xml", sb.ToString());
		}

		return target;
	}

	private void CreateDocumentTypeTreeItem(XDocument xDoc, ItemCollection target)
	{
		if (xDoc.DocumentType != null) CreateNodeTreeItem(xDoc.DocumentType, target);
	}

	private void CreateElementTreeItem(XElement element, ItemCollection target)
	{
		string prefix = element.GetPrefixOfNamespace(element.Name.Namespace) ?? "";

		if (prefix.Length > 0) prefix += ":";

		target = CreateTreeItem(target, typeof(XElement), prefix + element.Name.LocalName, element.Name.ToString());

		CreateAttributesTreeItems(element, target);

		foreach (XNode node in element.Nodes())
			if (node is XElement child) CreateElementTreeItem(child, target);
			else CreateNodeTreeItem(node, target);
	}

	private void CreateAttributesTreeItems(XElement element, ItemCollection target)
	{
		foreach (XAttribute attribute in element.Attributes())
		{
			string prefix = element.GetPrefixOfNamespace(attribute.Name.Namespace) ?? "";

			if (prefix.Length > 0) prefix += ":";

			CreateTreeItem(target, typeof(XAttribute), prefix + attribute.Name.LocalName + " = " + attribute.Value, attribute.Name.ToString());
		}
	}

	private ItemCollection CreateNodeTreeItem(XNode node, ItemCollection target)
	{
		string text = node switch
		{
			XText xt => xt.Value,
			XComment xc => xc.Value,
			XDocumentType xdt => xdt.Name,
			XProcessingInstruction xpi => xpi.Target,
			_ => ""
		};

		string? toolTip = node switch
		{
			XDocumentType xdt => $"{nameof(XDocumentType.PublicId)} = \"{xdt.PublicId}\", {nameof(XDocumentType.SystemId)} = \"{xdt.SystemId}\", {nameof(XDocumentType.InternalSubset)} = \"{xdt.InternalSubset}\", ",
			XProcessingInstruction xpi => xpi.Data,
			_ => null
		};

		return CreateTreeItem(target, node.GetType(), text, toolTip);
	}

	private ItemCollection CreateTreeItem(ItemCollection target, Type type, string text, string? toolTip)
	{
		toolTip = type.Name + (string.IsNullOrEmpty(toolTip) ? "" : ": " + toolTip);

		Grid grid = new Grid();

		grid.Children.Add(new Image() { Source = GetTypeImage(type), Height = 16, });
		grid.Children.Add(new TextBlock()
		{
			Text = type.Name[1..3].ToUpper(),
			Tag = toolTip,
			ToolTip = EscapeTextAndShowBlanks(toolTip),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			Foreground = new SolidColorBrush(Colors.White),
			FontFamily = new FontFamily("Arial,Helvetica"),
			FontSize = 8.25,
			Opacity = .75
		});

		StackPanel stackPanel = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(2) };

		stackPanel.Children.Add(grid);
		stackPanel.Children.Add(new TextBlock() { Text = EscapeTextAndShowBlanks(text), Tag = text, Padding = new Thickness(5, 0, 0, 0) });

		TreeViewItem child = new TreeViewItem() { Header = stackPanel };

		target.Add(child);

		return child.Items;
	}

	private static BitmapImage GetTypeImage(Type type)
		=> new BitmapImage(new Uri(_treeIconPath + type.Name switch
		{
			nameof(XElement) => "orange",
			nameof(XAttribute) => "green",
			nameof(XComment) => "turquoise",
			nameof(XText) => "blue",
			nameof(XCData) => "aqua",
			nameof(XDeclaration) => "purple",
			nameof(XDocumentType) => "pink",
			_ => "gray"
		} + ".png"));

	private void UpdateTreeTexts()
	{
		foreach (TreeViewItem child in XmlNodesTree.Items) UpdateNodeText(child);
	}

	private void UpdateNodeText(TreeViewItem item)
	{
		foreach (TreeViewItem child in item.Items) UpdateNodeText(child);

		if (item.Header is StackPanel sp && sp.Children.Count == 2)
		{
			if (sp.Children[0] is Grid grid && grid.Children.Count == 2 && grid.Children[1] is TextBlock typeName && typeName.Tag is string tooltip) typeName.ToolTip = EscapeTextAndShowBlanks(tooltip);
			if (sp.Children[1] is TextBlock txt && txt.Tag is string text) txt.Text = EscapeTextAndShowBlanks(text);
		}
	}



	/// <summary>
	///		Serialize the current <see cref="XDocument"/>
	///		object to the <see cref="XmlOutput"/> text
	///		control.
	/// </summary>
	private void WriteLinqToXml() => WriteLinqToXml(XmlOutput.Tag as XDocument);

	/// <summary>
	///		Serialize the specified <see cref="XDocument"/>
	///		object to the <see cref="XmlOutput"/> text
	///		control
	/// </summary>
	/// <param name="xDoc">
	///		<see cref="XDocument"/> object to be serialized.
	/// </param>
	private void WriteLinqToXml(XDocument? xDoc)
	{
		XmlOutput.Text = "";
		XmlOutput.Tag = xDoc;

		if (xDoc != null && ShowXml)
		{
			using MemoryStream ms = new MemoryStream(2046);

			if (_options.EnhancedWriter)
			{
				W.XmlWriterOptions options = new W.XmlWriterOptions()
				{
					OmitDuplicateNamespaces = _options.XmlSaveOptions.OmitDuplicateNamespaces,
					Indent = !_options.XmlSaveOptions.DisableFormatting,
				};

				if (_options.ReplaceTabs) options.IndentationString = new string(' ', _options.TabSize);

				new W.XmlWriter(xDoc, options).Write(ms);
			}
			else xDoc.Save(ms, (_options.XmlSaveOptions.DisableFormatting ? SaveOptions.DisableFormatting : SaveOptions.None) | (_options.XmlSaveOptions.OmitDuplicateNamespaces ? SaveOptions.OmitDuplicateNamespaces : SaveOptions.None));

			ms.Position = 0L;
			using StreamReader reader = new StreamReader(ms);

			string text = reader.ReadToEnd();

			XmlOutput.Text = _options.XmlSaveOptions.EscapeXml ? EscapeTextAndShowBlanks(text, true) : _showBlanks ? text.Replace(' ', '·') : text;
		}
	}



	/// <summary>
	///		Replaces the specified <see langword="string"/>'s
	///		non-printable ASCII characters with their C#
	///		escape sequence equivalents.
	/// </summary>
	/// <param name="text">
	///		Text to be escaped.
	/// </param>
	/// <returns>
	///		<see langword="string"/> with all non-printable
	///		ASCII characters being replaced with their C#
	///		escape sequence equivalents.
	/// </returns>
	private static string EscapeText(string text, bool addNewLine)
	{
		foreach ((string Key, string Value) in _constantChars) text = text.Replace(Key, Value);

		for (byte c = 0;c < 32;++c)
		{
			string rep = ((char)c).ToString();
			if (text.Contains(rep)) text = text.Replace(rep, $@"\x{c:x2}");
		}

		text = text.Replace(_newLineEscape, addNewLine ? Environment.NewLine : "");

		return text;
	}

	/// <summary>
	///		Replaces the specified <see langword="string"/>'s
	///		non-printable ASCII characters with their C#
	///		escape sequence equivalents and replaces the
	///		space character by a mid dot, depending on the
	///		<see cref="ShowBlanks"/> property.
	/// </summary>
	/// <param name="text">
	///		Text to be escaped.
	/// </param>
	/// <returns>
	///		<see langword="string"/> with all non-printable
	///		ASCII characters being replaced with their C#
	///		escape sequence equivalents.
	/// </returns>
	private string EscapeTextAndShowBlanks(string text) => EscapeTextAndShowBlanks(text, false);

	/// <summary>
	///		<para>
	///			Replaces the specified <see langword="string"/>'s
	///			non-printable ASCII characters with their C#
	///			escape sequence equivalents and replaces the
	///			space character by a mid dot, depending on the
	///		<see cref="ShowBlanks"/> property.
	///		</para>
	///		<para>
	///			If the <paramref name="addNewLine"/> argument is
	///			<see langword="true"/>, new-line characters will
	///			be both, retained and escaped.
	///		</para>
	/// </summary>
	/// <param name="text">
	///		Text to be escaped.
	/// </param>
	/// <param name="addNewLine">
	///		If <see langword="true"/>, new-line characters
	///		("<c>\n</c>") will be both, escaped and retained,
	///		so the resulting text will display with line-breaks;
	///		if <see langword="false"/>, new-line characters
	///		will be escaped just like all other non-printable
	///		ASCII characters, and the resulting text will always
	///		be displayed as a single line.
	/// </param>
	/// <returns>
	///		<see langword="string"/> with all non-printable
	///		ASCII characters being replaced with their C#
	///		escape sequence equivalents.
	/// </returns>
	private string EscapeTextAndShowBlanks(string text, bool addNewLine)
	{
		text = EscapeText(text, addNewLine);


		return _showBlanks ? text.Replace(' ', '·') : text;
	}



	/// <summary>
	///		Asks the user to confirm and optionally
	///		save the currently edited text if unsaved
	///		text changes are pending.
	/// </summary>
	/// <param name="sender">
	///		<see cref="Control"/> object that has triggered
	///		the event to be processed.
	/// </param>
	/// <param name="e">
	///		<see cref="RoutedEventArgs"/> object used to
	///		signal whether the user wants to cancel the
	///		current operation.
	/// </param>
	/// <returns>
	///		<see langword="true"/>, if the calling operation
	///		shall be processed; <see langword="false"/>, if
	///		the current operation is supposed to be cancelled
	///		without any changes being applied.
	/// </returns>
	private bool SaveDirty()
	{
		bool success = true;
		MessageBoxResult open = MessageBoxResult.Yes;

		if (_isDirty && !string.IsNullOrWhiteSpace(InputXml.Text))
		{
			open = MessageBox.Show(this, Messages.ConfirmSaveChangesMessage, Messages.ConfirmSaveChangesTitle, MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);

			if (open == MessageBoxResult.Yes) success = Save();
		}

		return open != MessageBoxResult.Cancel && success;
	}

	/// <summary>
	///		<para>
	///			Saves the current text content to a file.
	///		</para>
	///		<para>
	///			 If the current text is not associated
	///			 with a file path yet, runs a <see cref="SaveAs"/>
	///			 operation.
	///		</para>
	/// </summary>
	/// <returns>
	///		<see langword="true"/>, if the file was successfully
	///		saved; <see langword="false"/>, if the operation failed
	///		or the user cancelled the <see cref="SaveAs"/> operation.
	/// </returns>
	private bool Save()
	{
		bool success = true;

		if (_isDirty && !string.IsNullOrEmpty(InputXml.Text))
		{
			if (string.IsNullOrWhiteSpace(_filePath)) success = SaveAs();
			else
				try
				{
					File.WriteAllText(_filePath, InputXml.Text);
					SetWindowTitleAndDirtyFlag();
				}
				catch (IOException ex)
				{
					ErrorList.Text = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
					success = false;
				}
		}

		return success;
	}

	/// <summary>
	///		Saves the current text content to a new file.
	/// </summary>
	/// <returns>
	///		<see langword="true"/>, if the file was
	///		successfully saved; <see langword="false"/>,
	///		if the operation failed or the user cancelled
	///		the operation.
	/// </returns>
	private bool SaveAs()
	{
		bool success = true;

		if (!string.IsNullOrEmpty(InputXml.Text))
		{
			SaveFileDialog dialog = new SaveFileDialog() { DefaultExt = ".txt", FileName = Path.GetFileName(_filePath) ?? "", InitialDirectory = Path.GetDirectoryName(_filePath) ?? "", Filter = Messages.FileExtensions, Title = Messages.SaveFileTitle };

			if (dialog.ShowDialog(this) ?? false)
				try
				{
					using Stream stream = dialog.OpenFile();

					StreamWriter writer = new StreamWriter(stream);

					writer.Write(InputXml.Text);
					writer.Flush();

					AddCurrentFilePath(dialog.FileName);
				}
				catch (IOException ex)
				{
					ErrorList.Text = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
					success = false;
				}
			else success = false;
		}

		return success;
	}


	/// <summary>
	///		Opens the specified file, reads the file,
	///		evaluates its content and initializes all
	///		controls accordingly.
	/// </summary>
	/// <param name="filePath">
	///		String representing the path to the file
	///		to be opened.
	/// </param>
	private void OpenFile(string filePath)
	{
		using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

		OpenFile(filePath, stream);
	}

	/// <summary>
	///		Opens and reads the specified file stream,
	///		evaluates its content and initializes all
	///		controls accordingly.
	/// </summary>
	/// <param name="filePath">
	///		String representing the path to the file
	///		to be opened.
	/// </param>
	/// <param name="fileStream">
	///		Open <see cref="Stream"/> object from file
	///		to be read.
	/// </param>
	private void OpenFile(string filePath, Stream fileStream)
	{
		ReadContent(new StreamReader(fileStream).ReadToEnd());
		AddCurrentFilePath(filePath);
	}

	/// <summary>
	///		Reads the specified text, evaluates its content
	///		and initializes all controls accordingly.
	/// </summary>
	/// <param name="content">
	///		<see langword="string"/> specifying the full text
	///		to be evaluated and reflected in all controls.
	/// </param>
	private void ReadContent(string content)
	{
		bool invalidFile = false;

		content = ReplaceTabsWithSpaces(content);

		_applyCodeChange = false;
		if (!string.IsNullOrWhiteSpace(content))
			if (_codeRE.IsMatch(content)) UseCode = true;
			else
			{
				UseCode = false;

				if (content.Trim()[0] != '<') invalidFile = true;
			}
		_applyCodeChange = true;

		ResetControls(content);

		if (invalidFile) ErrorList.Text = Messages.InvalidFileContent;
		else CreateTreeFromTextInput();
	}

	/// <summary>
	///		If automatic tab to string conversion is
	///		enabled in application settings, replaces
	///		all occurences of a tab character with the
	///		appropriate number of spaces to reflect the
	///		original spacing. The spacing is specified
	///		in application settings.
	/// </summary>
	/// <param name="content">
	///		Text to be evaluated.
	/// </param>
	/// <returns>
	///		If automatic tab to string conversion
	///		is enabled in application settings, a
	///		<see langword="string"/> with all tabs
	///		being replaced by the appropriate number
	///		of spaces; otherwise, the unmodified
	///		<c><paramref name="content"/></c> argument.
	/// </returns>
	private string ReplaceTabsWithSpaces(string content)
	{
		if (_options.ReplaceTabs && content.Contains('\t'))
		{
			do
			{
				content = _tabRE.Replace(content, m => m.Groups[1] + new string(' ', _options.TabSize - m.Groups[1].Length % _options.TabSize));
			} while (content.Contains('\t'));
		}

		return content;
	}

	private void SetFindString()
	{
		string findString = SelectWholeWord();
		string escapedString = EscapeText(findString, false);

		if (escapedString != findString) _options.FindReplaceSettings.IsRegularExpression = true;

		_options.FindReplaceSettings.FindString = escapedString;
	}

	/// <summary>
	///		Selects a whole word in the <see cref="InputXml"/>
	///		text input control if no selection has been made.
	/// </summary>
	/// <returns>
	///		<see langword="string"/> representing the selected
	///		word.
	/// </returns>
	private string SelectWholeWord()
	{
		string text = InputXml.Text;
		int offset = InputXml.SelectionStart;
		int selLength = InputXml.SelectionLength;

		if (InputXml.SelectionLength == 0)
		{
			for (selLength = _wordCharsRE.Match(text[offset..]).Length;offset > 0 && _wordCharsRE.Match(text[(offset - 1)..]).Length > selLength;--offset) ++selLength;

			InputXml.SelectionStart = offset;
			InputXml.SelectionLength = selLength;
		}

		return text[offset..(offset + selLength)];
	}



	private static void OnUseCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is MainWindow mWnd && mWnd._applyCodeChange) mWnd.ResetControls(e.NewValue is bool nv && nv);
	}

	private static void OnShowXmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is MainWindow mWnd && (bool)e.NewValue) mWnd.WriteLinqToXml();
	}



	private void FileMru_Click(object sender, RoutedEventArgs e)
	{
		if (sender is MenuItem item && item.Tag is string filePath && !string.IsNullOrWhiteSpace(filePath))
		{
			if (File.Exists(filePath))
			{
				if (SaveDirty())
				{
					try
					{
						OpenFile(filePath);
					}
					catch (IOException ex)
					{
						ErrorList.Text = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
					}
				}
			}
			else if (MessageBox.Show(this, ReplaceNewLine(string.Format(Messages.MruFileNotFoundMessage, filePath)), Messages.MruFileNotFoundTitle, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
			{
				_options.MruFileList.Remove(filePath);

				SetMruFileMenu();
			}
		}
	}

	private void New_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (SaveDirty()) ResetControls(UseCode);
	}

	private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (SaveDirty())
		{
			OpenFileDialog dialog = new OpenFileDialog() { CheckFileExists = true, DefaultExt = ".txt", FileName = Path.GetFileName(_filePath) ?? "", InitialDirectory = Path.GetDirectoryName(_filePath) ?? "", Filter = Messages.FileExtensions, Title = Messages.OpenFileTitle };

			if (dialog.ShowDialog(this) ?? false)
			{
				try
				{
					using Stream stream = dialog.OpenFile();

					OpenFile(dialog.FileName, stream);
				}
				catch (IOException ex)
				{
					ErrorList.Text = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
					e.Handled = true; // signal cancellation
				}
			}
		}
	}

	private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		if (InputXml != null)
		{
			e.CanExecute = _isDirty && !string.IsNullOrWhiteSpace(InputXml.Text);
			e.Handled = true;
		}
	}

	private void Save_Executed(object sender, RoutedEventArgs e) => Save();

	private void SaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		if (InputXml != null)
		{
			e.CanExecute = !string.IsNullOrWhiteSpace(InputXml.Text);
			e.Handled = true;
		}
	}

	private void SaveAs_Executed(object sender, RoutedEventArgs e) => SaveAs();

	private void Quit_Executed(object sender, ExecutedRoutedEventArgs e) => Close();

	private void Focus_Executed(object sender, ExecutedRoutedEventArgs e) => InputXml.Focus();

	private void Blanks_Executed(object sender, ExecutedRoutedEventArgs e) => CreateTreeFromTextInput();

	private void Linq_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (e.Parameter == null) UseCode = !UseCode;

		if (SaveDirty()) ViewMenuUseCode.IsChecked = UseCode;
		else ViewMenuUseCode.IsChecked = UseCode;
	}

	private void ShowXml_Executed(object sender, ExecutedRoutedEventArgs e) { if (e.Parameter == null) ShowXml = !ShowXml; }

	private void Options_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		O.OptionsWindow options = new O.OptionsWindow(_options) { Owner = this };

		if (options.ShowDialog() ?? false)
		{
			bool replaceTabs = _options.ReplaceTabs;

			_options = options.Options;

			if (_options.ReplaceTabs && _options.ReplaceTabs != replaceTabs) ReadContent(InputXml.Text);

			SetMruFileMenu();
			WriteLinqToXml();
		}
	}

	private void About_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		A.AboutWindow about = new A.AboutWindow() { Owner = this };

		about.ShowDialog();
	}



	private void InputXml_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (_applyTextChange)
		{
			if (!_isDirty) SetWindowTitleAndDirtyFlag(true);
			if (!UseCode) CreateTreeFromTextInput();
		}
	}

	private void InputXml_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (_options.ReplaceTabs && e.Key == Key.Tab && e.KeyboardDevice.Modifiers == ModifierKeys.None)
		{
			int selStart = InputXml.SelectionStart;
			int newLineLen = Environment.NewLine.Length;

			if (InputXml.SelectionLength > 0) InputXml.SelectedText = "";

			string[] lines = InputXml.Text.Split(Environment.NewLine);
			int idx = 0;

			for (int l = 0;l < lines.Length;++l)
			{
				string line = lines[l];

				if (idx <= selStart && idx + line.Length >= selStart)
				{
					int offset = selStart - idx;
					string spaces = new string(' ', _options.TabSize - offset % _options.TabSize);

					lines[l] = line[..offset] + spaces + line[offset..];
					selStart += spaces.Length;
					break;
				}

				idx += line.Length + newLineLen;
			}

			_applyTextChange = false;
			InputXml.Text = string.Join(Environment.NewLine, lines);
			_applyTextChange = true;

			InputXml.SelectionStart = selStart;

			e.Handled = true;
		}
	}

	private void InputXml_PreviewDragOver(object sender, DragEventArgs e)
	{
		string[] formats = e.Data.GetFormats();

		if ((e.AllowedEffects & DragDropEffects.Copy) == DragDropEffects.Copy && formats.Intersect(_dragDropFileFormats).Any())
		{
			e.Effects = DragDropEffects.Copy;
			e.Handled = true;
		}
	}

	private void InputXml_Drop(object sender, DragEventArgs e)
	{
		if ((e.AllowedEffects & DragDropEffects.Copy) == DragDropEffects.Copy)
		{
			foreach (string format in _dragDropFileFormats)
				if (e.Data.GetDataPresent(format) && e.Data.GetData(format) is string[] filePaths && filePaths.Length > 0 && !string.IsNullOrWhiteSpace(filePaths[0]))
				{
					if (SaveDirty()) OpenFile(filePaths[0]);

					e.Handled = true;
					break;
				}
		}
	}

	private void InputXml_SizeChanged(object sender, SizeChangedEventArgs e) => SetFindControlWidth(FindReplaceBox.ActualWidth);



	private void InputXml_Cut(object sender, ExecutedRoutedEventArgs e)
	{
		string content = InputXml.Text;

		Clipboard.SetText(content[InputXml.SelectionStart..(InputXml.SelectionStart + InputXml.SelectionLength + 1)]);

		InputXml.Text = content[..InputXml.SelectionStart] + content[(InputXml.SelectionStart + InputXml.SelectionLength)..];
	}

	private void Cut_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = InputXml != null && InputXml.SelectionLength > 0;

	private void InputXml_Copy(object sender, ExecutedRoutedEventArgs e)
		=> Clipboard.SetText(InputXml.Text[InputXml.SelectionStart..(InputXml.SelectionStart + InputXml.SelectionLength + 1)]);

	private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = InputXml != null && InputXml.SelectionLength > 0;

	private void InputXml_Paste(object sender, ExecutedRoutedEventArgs e)
	{
		if (Clipboard.ContainsText())
		{
			const char marker = '\u0006';
			string content = InputXml.Text;

			content = ReplaceTabsWithSpaces(content[..InputXml.SelectionStart] + Clipboard.GetText() + marker + content[(InputXml.SelectionStart + InputXml.SelectionLength)..]);

			int selStart = content.IndexOf(marker);

			InputXml.Text = content.Replace(marker.ToString(), "");
			InputXml.Select(selStart, 0);
		}
	}

	private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Clipboard.ContainsText();



	private void Find_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = InputXml != null && InputXml.Text.Length > 0;

	private void Find_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		SetFindString();
		FindReplaceBox.Visibility = Visibility.Visible;
		FindReplaceCtrl.Mode = FindReplaceMode.Find;
	}

	private void FindNext_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = e.CanExecute = InputXml != null && InputXml.Text.Length > 0 && _options.FindReplaceSettings.FindString.Length > 0;

	private void FindNext_Executed(object sender, ExecutedRoutedEventArgs e)
		=> new TextFindReplacer(InputXml, _options.FindReplaceSettings).FindNext();

	private void FindPrevious_CanExecute(object sender, CanExecuteRoutedEventArgs e) => FindNext_CanExecute(sender, e);

	private void FindPrevious_Executed(object sender, ExecutedRoutedEventArgs e)
		=> new TextFindReplacer(InputXml, _options.FindReplaceSettings).FindPrevious();

	private void QuickFindNext_CanExecute(object sender, CanExecuteRoutedEventArgs e) => Find_CanExecute(sender, e);

	private void QuickFindNext_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		SetFindString();
		new TextFindReplacer(InputXml, _options.FindReplaceSettings).FindNext();
	}

	private void QuickFindPrevious_CanExecute(object sender, CanExecuteRoutedEventArgs e) => Find_CanExecute(sender, e);

	private void QuickFindPrevious_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		SetFindString();
		new TextFindReplacer(InputXml, _options.FindReplaceSettings).FindPrevious();
	}

	private void Replace_CanExecute(object sender, CanExecuteRoutedEventArgs e) => Find_CanExecute(sender, e);

	private void Replace_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		SetFindString();
		FindReplaceBox.Visibility = Visibility.Visible;
		FindReplaceCtrl.Mode = FindReplaceMode.Replace;
	}



	private void FindReplace_KeyUp(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape && e.KeyboardDevice.Modifiers == ModifierKeys.None)
		{
			InputXml.Focus();
			FindReplaceBox.Visibility = Visibility.Hidden;
		}
	}

	private void FindReplace_StartResize(object sender, MouseButtonEventArgs e)
	{
		if (e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left)
		{
			ResizeBar.CaptureMouse();
			_draggingFindDlg = true;
			_findMouseX = e.GetPosition(InputXml).X;
			_findWidth = FindReplaceBox.ActualWidth;
		}
	}

	private void FindReplace_Resize(object sender, MouseEventArgs e)
	{
		if (_draggingFindDlg) SetFindControlWidth(_findWidth + _findMouseX - e.GetPosition(InputXml).X);
	}

	private void FindReplace_EndResize(object sender, MouseButtonEventArgs e)
	{
		if (_draggingFindDlg)
		{
			ResizeBar.ReleaseMouseCapture();
			_draggingFindDlg = false;
		}
	}



	private void EscapeXml_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (e.Parameter == null) _options.XmlSaveOptions.EscapeXml = !_options.XmlSaveOptions.EscapeXml;
		WriteLinqToXml();
	}

	private void NoDupNsXml_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (e.Parameter == null) _options.XmlSaveOptions.OmitDuplicateNamespaces = !_options.XmlSaveOptions.OmitDuplicateNamespaces;
		WriteLinqToXml();
	}

	private void NoFormatXml_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (e.Parameter == null) _options.XmlSaveOptions.DisableFormatting = !_options.XmlSaveOptions.DisableFormatting;
		WriteLinqToXml();
	}



	private void Always_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

	private void Never_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.Handled = true;

	private void Submit_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (UseCode)
			if (_codeRE.IsMatch(InputXml.Text)) CreateTreeFromTextInput();
			else ErrorList.Text = Messages.InvalidCode;
	}

	private void Window_Closing(object sender, CancelEventArgs e) => e.Cancel = !SaveDirty();

	private void Window_Closed(object sender, EventArgs e) => _options.Save();
}