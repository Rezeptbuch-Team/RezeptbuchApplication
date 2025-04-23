using System.Collections.ObjectModel;
using ApplicationCore.Common.Types;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GUI.ViewModel;

public partial class ShowRecipeViewModel : ObservableObject
{
    public ShowRecipeViewModel()
    {
        RecipeName = "Recipe Name";
        RecipeTime = 10;
        RecipeIngredients = [new Ingredient("Ingredient1", 20), new Ingredient("Ingredient2", 10), new Ingredient("Ingredient3", 14)];
        RecipeDescription = "Recipe Description";
        RecipeImage = "dotnet_bot.png";
    }
    
    [ObservableProperty] 
    private string _recipeName;
    
    [ObservableProperty]
    private int _recipeTime;
    
    [ObservableProperty]
    private ObservableCollection<Ingredient> _recipeIngredients;
    
    [ObservableProperty]
    private string _recipeDescription;

    [ObservableProperty] 
    private ImageSource? _recipeImage;
}