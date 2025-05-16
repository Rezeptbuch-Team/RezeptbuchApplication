using GUI.ViewModel;
using Microsoft.Maui.Controls;

namespace GUI.View;

public partial class NavigateThroughRecipeView : ContentPage
{
    public NavigateThroughRecipeView(NavigateThroughRecipeViewModel vm)
    {
        BindingContext = vm;
        
        InitializeComponent();
    }
}