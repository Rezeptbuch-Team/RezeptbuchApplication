using System.Collections.ObjectModel;
using ApplicationCore.Common.Types;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;

namespace GUI.ViewModel;

public partial class ShowRecipeViewModel : ObservableObject
{
    public ShowRecipeViewModel()
    {
        RecipeName = "Recipe Name";
        RecipeTime = 10;
        RecipeIngredients = [
        new Ingredient() {
            Name="Ingredient1",
            Amount=500,
            Unit="ml"
        },
        new Ingredient() {
            Name="Ingredient2",
            Amount=200,
            Unit="g"
        },
        new Ingredient() {
            Name="Ingredient3",
            Amount=60,
            Unit="g"
        }];
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