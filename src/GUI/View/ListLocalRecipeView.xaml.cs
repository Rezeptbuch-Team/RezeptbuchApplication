using GUI.ViewModel;
using Microsoft.Maui.Controls;

namespace GUI.View;

public partial class ListLocalRecipeView : ContentPage
{
    private readonly ListLocalRecipeViewModel _viewModel;
    
    public ListLocalRecipeView(ListLocalRecipeViewModel vm)
    {
        BindingContext = vm;
        _viewModel = vm;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        _ = _viewModel.RefreshRecipes();
    }
}