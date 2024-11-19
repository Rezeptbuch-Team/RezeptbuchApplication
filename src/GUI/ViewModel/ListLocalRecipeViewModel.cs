using System.Collections.ObjectModel;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GUI.ViewModel;

public partial class ListLocalRecipeViewModel : ObservableObject
{
    
    public ListLocalRecipeViewModel()
    {
        Text = "Test DataBinding";
        Recipes = new();
        for (int i = 0; i < 10; i++)
        {
            Recipes.Add($"Titel {i}, Description {i}");
        }
    }
    
    [ObservableProperty] 
    private ObservableCollection<string> recipes;

    [ObservableProperty] private string text;
}