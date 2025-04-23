using System.Collections.ObjectModel;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GUI.ViewModel;

public partial class ListLocalRecipeViewModel : ObservableObject
{
    private readonly IOnlineRecipeListService _recipeListService;
    
    [ObservableProperty] 
    private List<RecipeEntry> _recipeEntries;
    
    [ObservableProperty] private string _text;
    
    public ListLocalRecipeViewModel(IOnlineRecipeListService onlineRecipeListService)
    {
        _recipeListService = onlineRecipeListService;
        Text = "Test DataBinding";
        RecipeEntries = [];
        RefreshRecipes();
    }

    private async Task RefreshRecipes()
    {
        RecipeEntries = await _recipeListService.GetOnlineRecipeList(new Filter(OrderBy.TITLE, Order.ASCENDING, ["category1", "category2"], 10, 0));
    }
}