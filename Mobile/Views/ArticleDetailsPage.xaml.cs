using Mobile.ViewModels;

namespace Mobile.Views;

public partial class ArticleDetailsPage : ContentPage
{
    public ArticleDetailsPage(ArticleDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
