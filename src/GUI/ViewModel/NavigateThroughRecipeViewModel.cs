using CommunityToolkit.Mvvm.ComponentModel;

namespace GUI.ViewModel;

public partial class NavigateThroughRecipeViewModel : ObservableObject
{
    public NavigateThroughRecipeViewModel()
    {
        StepDescription = "Description";
    }
    
    [ObservableProperty] 
    private string _stepDescription;
}