using ExpenseTrackerApp.ViewModels;
using Microsoft.Maui.Graphics;

namespace ExpenseTrackerApp.Views;

public partial class ReportsPage : ContentPage
{
    private readonly OverviewViewModel _vm;

    public ReportsPage(OverviewViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        DonutChart.Drawable = new DonutChartDrawable(_vm);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
        DonutChart.Invalidate();
    }
}

// วาดกราฟโดนัทแบบ native ด้วย Microsoft.Maui.Graphics ไม่ต้องพึ่ง library ภายนอก
public class DonutChartDrawable : IDrawable
{
    private readonly OverviewViewModel _vm;

    public DonutChartDrawable(OverviewViewModel vm) => _vm = vm;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var total = _vm.CategorySummaries.Sum(c => c.Amount);
        if (total <= 0) return;

        float centerX = dirtyRect.Width / 2;
        float centerY = dirtyRect.Height / 2;
        float radius = Math.Min(centerX, centerY) - 10;
        float strokeWidth = 28;

        float startAngle = -90;
        foreach (var item in _vm.CategorySummaries)
        {
            float sweep = (float)(item.Amount / total) * 360f;
            canvas.StrokeColor = Color.FromArgb(item.ColorHex);
            canvas.StrokeSize = strokeWidth;
            canvas.StrokeLineCap = LineCap.Butt;
            canvas.DrawArc(centerX - radius, centerY - radius, radius * 2, radius * 2, startAngle, startAngle + sweep, false, false);
            startAngle += sweep;
        }

        canvas.FontColor = Color.FromArgb("#5A4020");
        canvas.FontSize = 16;
        canvas.DrawString(total.ToString("N0"), centerX, centerY, HorizontalAlignment.Center);
    }
}
