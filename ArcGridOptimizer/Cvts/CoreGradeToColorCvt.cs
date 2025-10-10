using ArcGridOptimizer.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ArcGridOptimizer.Cvts
{
    public class CoreGradeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is eCoreGrade grade)
            {
                return grade switch
                {
                    eCoreGrade.Hero => Colors.Purple,
                    eCoreGrade.Legend => Colors.Gold,
                    eCoreGrade.Relic => Colors.Orange,
                    eCoreGrade.Ancient => Colors.Moccasin,
                    _ => Colors.Transparent
                };
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
