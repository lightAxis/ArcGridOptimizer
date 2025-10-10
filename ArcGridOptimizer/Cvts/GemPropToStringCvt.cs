using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ArcGridOptimizer.Enums;
using ArcGridOptimizer.ViewModels;
using ArcGridOptimizer.ViewModels.Items;

namespace ArcGridOptimizer.Cvts
{
    public class GemPropToStringCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GemProperty gp)
            {
                var cvt = new Enum2DescCvt();
                string typestr = cvt.Convert2(gp.Type, typeof(eGemProperty));
                if (gp.Type == eGemProperty.None)
                    return "";
                return $"{typestr} {gp.Value}";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
             => Binding.DoNothing;
    }
}
