using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace XDocumentExplorer.Converters
{
	internal class VisibilityConverter : IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool isTrue && isTrue ? Visibility.Visible : Visibility.Collapsed;

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is Visibility isVisible && isVisible == Visibility.Visible;
	}
}
