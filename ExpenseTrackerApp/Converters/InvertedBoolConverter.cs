using System.Globalization;

namespace ExpenseTrackerApp.Converters;

// ใช้กลับค่า bool สำหรับ IsVisible เช่น ซ่อนฟิลด์บัตรเครดิตเมื่อเลือกประเภทอื่น
public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value!;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value!;
}
