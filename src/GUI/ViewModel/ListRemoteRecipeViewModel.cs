using System.Collections.ObjectModel;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GUI.ViewModel;

public partial class ListRemoteRecipeViewModel : ListRecipeViewModel
{
    private readonly IOnlineRecipeListService _recipeListService;
    
    public ListRemoteRecipeViewModel(IOnlineRecipeListService recipeListService) : base(recipeListService)
    {
        _recipeListService = recipeListService;
        _ = RefreshAvailableFilterOptions();
    }

    private async Task RefreshAvailableFilterOptions()
    {
        AvailableFilterOptions = await _recipeListService.GetCategories(10, 0);
    }
}