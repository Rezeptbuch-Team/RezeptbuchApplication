using GUI.ViewModel;
using Microsoft.Maui.Controls;

namespace GUI.View;

public partial class ListRemoteRecipeView : ContentPage
{
    private readonly ListRemoteRecipeViewModel _viewModel;
    
    public ListRemoteRecipeView(ListRemoteRecipeViewModel vm)
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