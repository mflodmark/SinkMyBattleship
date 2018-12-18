using SinkMyBattleshipWPF.Models;
using SinkMyBattleshipWPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace SinkMyBattleshipWPF.Converters
{
    public class FireConverter : IValueConverter
    {
      
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var vm = (Dictionary<string, int>)value;

            switch (vm[parameter.ToString()])
            {
                case 0:
                    return Brushes.LightBlue;
                case 1:
                    return Brushes.Red;
                case 2:
                    return Brushes.White;
                default:
                    return Brushes.LightBlue;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
