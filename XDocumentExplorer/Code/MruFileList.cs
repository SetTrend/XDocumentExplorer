using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using XDocumentExplorer.Converters.Json;

namespace XDocumentExplorer.Code
{
	[JsonConverter(typeof(MruFileListConverter))]
	public class MruFileList
	{
		private readonly List<string> _mruFiles = [];
		private byte _itemCapacity;



		/// <summary>
		///		Gets or sets a value determining
		///		the maximum number of file path
		///		items in the list of most recently
		///		used files.
		/// </summary>
		public byte Capacity
		{
			get => _itemCapacity;

			set
			{
				_itemCapacity = value switch
				{
					< 1 => 1,
					> 10 => 10,
					_ => value
				};

				TruncateSurplusEntries();
			}
		}

		/// <summary>
		///		Gets a read-only collection of file
		///		path strings representing the most
		///		recently used files.
		/// </summary>
		public IReadOnlyList<string> MruFiles => _mruFiles.AsReadOnly();



		/// <summary>
		///		Initializes a new, empty <see cref="MruFileList"/>
		///		object with the specified capacity.
		/// </summary>
		/// <param name="capacity">
		///		Maximum number of file path items this object will hold.
		/// </param>
		public MruFileList(byte capacity) => _itemCapacity = capacity;

		/// <summary>
		///		Initializes a new <see cref="MruFileList"/>
		///		object with the specified capacity and list
		///		of recently used file paths.
		/// </summary>
		/// <param name="capacity">
		///		Maximum number of file path items this object
		///		will hold.
		/// </param>
		/// <param name="mruFiles">
		///		File path strings representing the most
		///		recently used files.
		/// </param>
		public MruFileList(byte capacity, List<string> mruFiles) : this(capacity)
		{
			_mruFiles.AddRange(mruFiles);

			RemoveDuplicateMruFileItems();
			TruncateSurplusEntries();
		}

		/// <summary>
		///		Initializes a new <see cref="MruFileList"/> object
		///		from another <see cref="MruFileList"/> object.
		/// </summary>
		/// <param name="other">
		///		<see cref="MruFileList"/> object to copy its
		///		fields from.
		/// </param>
		/// <remarks>
		///		The list of file path items will be copied, so
		///		both <see cref="MruFileList"/> objects will retain
		///		their own copy of file paths.
		/// </remarks>
		public MruFileList(MruFileList other)
		{
			_itemCapacity = other._itemCapacity;

			_mruFiles.AddRange(other._mruFiles);
		}



		/// <summary>
		///		Adds a new file path string to
		///		the top of the most recently
		///		used files collection.
		/// </summary>
		/// <param name="mruFile">
		///		A <see langword="string"/>
		///		specifying a file path.
		/// </param>
		public void Add(string mruFile)
		{
			_mruFiles.Insert(0, mruFile);

			RemoveDuplicateMruFileItems();
			TruncateSurplusEntries();
		}

		/// <summary>
		///		Removes the specified file path from the
		///		list of most recently used files.
		/// </summary>
		/// <param name="mruFile"></param>
		/// <remarks>
		///		Comparison is performed case insensitive.
		/// </remarks>
		public void Remove(string mruFile)
		{
			foreach (string filePath in _mruFiles)
				if (filePath.Equals(mruFile, StringComparison.CurrentCultureIgnoreCase))
				{
					_mruFiles.Remove(filePath);
					break;
				}
		}



		/// <summary>
		///		Removes duplicate file path entries from
		///		the list of most recently used files.
		/// </summary>
		/// <remarks>
		///		Comparison is performed case insensitive.
		///		If duplicate entries are found, the item
		///		with lowest index in the list will be
		///		retained.
		/// </remarks>
		private void RemoveDuplicateMruFileItems()
		{
			for (int i = _mruFiles.Count - 1;i > 0;--i)
				for (int j = 0;j < i;++j)
					if (_mruFiles[i].Equals(_mruFiles[j], StringComparison.CurrentCultureIgnoreCase))
					{
						_mruFiles.RemoveAt(i);
						break;
					}
		}

		/// <summary>
		///		Removes excessive number of file
		///		path entries from the list of most
		///		recently used files.
		/// </summary>
		private void TruncateSurplusEntries()
		{
			int count = _mruFiles.Count;

			if (count > _itemCapacity) _mruFiles.RemoveRange(_itemCapacity, count - _itemCapacity);
		}
	}
}
