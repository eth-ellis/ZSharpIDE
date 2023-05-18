using Microsoft.UI.Xaml.Data;
using System;
using System.IO;

namespace ZSharpIDE.Converters
{
    public class FileInfoNoExtensionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is FileInfo fileInfo)
            {
                return Path.GetFileNameWithoutExtension(fileInfo.Name);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}