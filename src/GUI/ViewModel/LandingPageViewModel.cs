using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GUI.View;

namespace GUI.ViewModel;

public partial class LandingPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string text = "Landing Page";
    
    
    [RelayCommand]
    private async Task OpenListLocalRecipe()
    {
        await Shell.Current.GoToAsync($"/{nameof(ListLocalRecipeView)}");
    }
}