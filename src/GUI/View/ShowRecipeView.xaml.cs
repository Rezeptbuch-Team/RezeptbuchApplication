using GUI.ViewModel;
using Microsoft.Maui.Controls;

namespace GUI.View;

public partial class ShowRecipeView : ContentPage
{
    public ShowRecipeView(ShowRecipeViewModel vm)
    {
        BindingContext = vm;
        
        InitializeComponent();
    }
}