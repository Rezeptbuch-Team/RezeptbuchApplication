using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GUI.View;
using Microsoft.Maui.Controls;

namespace GUI.ViewModel;

public partial class LandingPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _text = "Landing Page";
    
    
    [RelayCommand]
    private async Task OpenListLocalRecipe()
    {
        await Shell.Current.GoToAsync($"/{nameof(ListLocalRecipeView)}");
    }
}