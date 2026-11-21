using Mobile.ViewModels;

namespace Mobile.Views;

public partial class DataHistoryPage : ContentPage
{
    public DataHistoryPage(DataHistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
