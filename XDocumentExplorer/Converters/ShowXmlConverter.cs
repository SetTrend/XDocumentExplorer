using System;
using System.Globalization;
using System.Windows.Data;

namespace XDocumentExplorer.Converters
{
	internal class ShowXmlConverter : IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool isTrue && isTrue ? 1 : parameter;

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is not int span || span > 1;
	}
}
