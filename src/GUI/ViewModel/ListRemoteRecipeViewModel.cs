using System.Collections.ObjectModel;
using ApplicationCore.Common.Types;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GUI.ViewModel;

public partial class ListRemoteRecipeViewModel : ObservableObject
{
    public ListRemoteRecipeViewModel()
    {
        Recipes = new();
        List<string> list = new();
        for (int i = 0; i < 10; i++)
        {
            Recipes.Add(new Recipe(i.ToString(), $"Title {i}", $"Description {i}"));
        }
    }
    
    [ObservableProperty] 
    private ObservableCollection<Recipe> recipes;
}