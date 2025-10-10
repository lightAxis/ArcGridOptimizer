using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ArcGridOptimizer.Cvts
{
    public class Enum2DescCvt: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert2(value, targetType);
        }

        public string Convert2(object value, Type targetType)
        {
            if (value is Enum e)
            {
                var fi = e.GetType().GetField(e.ToString());
                var attr = (DescriptionAttribute?)Attribute.GetCustomAttribute(fi!, typeof(DescriptionAttribute));
                return attr?.Description ?? e.ToString();
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertBack2(value, targetType);
        }

        public object ConvertBack2(object value, Type targetType)
        {
            var text = value?.ToString();
            if (string.IsNullOrWhiteSpace(text))
                return Binding.DoNothing;

            var enumType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            var isNullable = Nullable.GetUnderlyingType(targetType) != null;

            if (!enumType.IsEnum)
                return Binding.DoNothing;

            foreach (var field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var desc = field.GetCustomAttribute<DescriptionAttribute>()?.Description;
                if (!string.IsNullOrEmpty(desc) &&
                    string.Equals(desc, text, StringComparison.OrdinalIgnoreCase))
                {
                    return Enum.Parse(enumType, field.Name);
                }
            }

            if (Enum.GetNames(enumType).Any(n => string.Equals(n, text, StringComparison.OrdinalIgnoreCase)))
                return Enum.Parse(enumType, text!, ignoreCase: true);

            if (Enum.TryParse(enumType, text, ignoreCase: true, out var parsed))
                return parsed!;

            return isNullable ? null : Binding.DoNothing;
        }
           
    }
}
