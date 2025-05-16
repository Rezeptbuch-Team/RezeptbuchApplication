using GUI.ViewModel;
using Microsoft.Maui.Controls;

namespace GUI.View;

public partial class ListRemoteRecipeView : ContentPage
{
    public ListRemoteRecipeView(ListRemoteRecipeViewModel vm)
    {
        BindingContext = vm;
        
        InitializeComponent();
    }
}