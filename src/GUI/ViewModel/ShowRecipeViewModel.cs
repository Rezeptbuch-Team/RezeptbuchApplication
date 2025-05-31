using System.Collections.ObjectModel;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;

namespace GUI.ViewModel;

[QueryProperty(nameof(Recipe), "Recipe")]
public partial class ShowRecipeViewModel : ObservableObject, IQueryAttributable
{
    public Recipe Recipe
    {
        set
        {
            _recipe = value;
            Servings = "1";
            ImagePath = value.ImagePath;
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(ImagePath));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(CookingTime));
        }
    }

    private Recipe? _recipe;
    
    public string? Title => _recipe?.Title;

    public string ImagePath
    {
        get =>
            _recipe != null ? _recipe!.ImagePath : "";
        set => _recipe!.ImagePath = value;
    }

    public string? Description => _recipe?.Description;
    
    public string? Servings
    {
        get => _servings.ToString();
        set
        {
            if(!int.TryParse(value, out var servings)) return;
            SetProperty(ref _servings, servings);
            Ingredients = _recipe == null ? null : new ObservableCollection<Ingredient>(_recipe.GetIngredients(servings));
        }
    }

    private int? _servings;
    
    public int? CookingTime => _recipe?.CookingTime;
    
    [ObservableProperty]
    private ObservableCollection<Ingredient>? _ingredients;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Recipe = (query[nameof(Recipe)] as Recipe)!;
    }
}