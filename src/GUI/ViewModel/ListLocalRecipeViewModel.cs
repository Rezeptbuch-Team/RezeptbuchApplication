using System.Collections.ObjectModel;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GUI.ViewModel;

public partial class ListLocalRecipeViewModel : ObservableObject
{

    public class Recipe(string Titel, string Description);
    
    public ListLocalRecipeViewModel()
    {
        Recipes = new();
        List<string> categories = new();
        for (int i = 0; i < 10; i++)
        {
            Recipes.Add(new Recipe($"Titel {i}", $"Description {i}"));
        }
    }
    
    [ObservableProperty] 
    private ObservableCollection<Recipe> recipes;
}