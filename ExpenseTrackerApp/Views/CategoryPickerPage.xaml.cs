using ExpenseTrackerApp.Services;

namespace ExpenseTrackerApp.Views;

public partial class CategoryPickerPage : ContentPage
{
    private readonly IDatabaseService _db;

    public CategoryPickerPage(IDatabaseService db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        CategoryList.ItemsSource = await _db.GetCategoriesAsync();
    }
}
