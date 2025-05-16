using GUI.ViewModel;
using Microsoft.Maui.Controls;

namespace GUI.View;

public partial class ListLocalRecipeView : ContentPage
{
    public ListLocalRecipeView(ListLocalRecipeViewModel vm)
    {
        BindingContext = vm;
        
        InitializeComponent();
    }
}