using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GUI.ViewModel;

public partial class ListRecipeViewModel : ObservableObject
{
    private readonly IOnlineRecipeListService _recipeListService;
    
    [ObservableProperty] 
    private List<RecipeEntry> _recipeEntries;
    
    public ListRecipeViewModel(IOnlineRecipeListService onlineRecipeListService)
    {
        _recipeListService = onlineRecipeListService;
        RecipeEntries = [];
        RefreshRecipes(new Filter(OrderBy.TITLE, Order.ASCENDING, ["category1", "category2"], 10, 0));
    }

    private async Task RefreshRecipes(Filter filter)
    {
        RecipeEntries = await _recipeListService.GetOnlineRecipeList(filter);
    }
}