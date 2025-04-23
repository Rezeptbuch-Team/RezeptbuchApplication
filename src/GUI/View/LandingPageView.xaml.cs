using GUI.ViewModel;
using Microsoft.Maui.Controls;

namespace GUI.View;

public partial class LandingPageView : ContentPage
{
    public LandingPageView(LandingPageViewModel vm)
    {
        BindingContext = vm;
        
        InitializeComponent();
    }
}