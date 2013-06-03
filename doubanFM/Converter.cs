using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace doubanFM
{
    class IsLike2ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string imagePath = "Images/UnLike.png";
            try
            {
                string isLike = (string)value;
                if (isLike == "1")
                    imagePath = "Images/Like.png";
            }
            catch
            { }
            return imagePath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    class Array2StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            StringBuilder retString = new StringBuilder("");
            try
            {
                IEnumerable<string> array = (IEnumerable<string>)value;
                foreach (string s in array)
                    retString.Append(s + @"/");
            }
            catch
            { }
            return retString.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string retString = string.Empty;
            try
            {
                string s = (string)value;
                retString = string.Format("共{0}首歌", s);
            }
            catch
            { }
            return retString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
