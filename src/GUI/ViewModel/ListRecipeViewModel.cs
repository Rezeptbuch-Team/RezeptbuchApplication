using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GUI.ViewModel;

public partial class ListRecipeViewModel : ObservableObject
{
    private readonly IRecipeListService _recipeListService;
    
    [ObservableProperty] 
    private List<RecipeEntry> _recipeEntries;
    
    public ListRecipeViewModel(IRecipeListService recipeListService)
    {
        _recipeListService = recipeListService;
        RecipeEntries = [];
        RefreshRecipes(new Filter(OrderBy.TITLE, Order.ASCENDING, ["category1", "category2"], null,10, 0));
    }

    private async Task RefreshRecipes(Filter filter)
    {
        RecipeEntries = await _recipeListService.GetRecipeList(filter);
    }
}