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
        for (int i = 0; i < 100; i++)
        {
            Recipes.Add(new Recipe(i.ToString(), $"Title {i}", $"Description {i}"));
        }
    }
    
    [ObservableProperty] 
    private ObservableCollection<Recipe> _recipes;
    
    [ObservableProperty] private string _text;
}