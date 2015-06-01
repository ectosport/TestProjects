using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace DirectorySizes
{
    [ValueConversion(typeof(object), typeof(int))]
    class DirToValueConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            dirData _dirData = value as dirData;
            if (_dirData == null) return 0;
            //dirData _dirData = (dirData)System.Convert.ChangeType(value, typeof(dirData));

            if (_dirData.dirName == "..")
                return -1;
            else if (_dirData.isDir == true)
                return 1;
            else
                return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack not supported");
        }

        #endregion
    }
}
