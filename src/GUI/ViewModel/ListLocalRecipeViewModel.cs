using System.Collections.ObjectModel;
using System.Diagnostics;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GUI.View;

namespace GUI.ViewModel;

public partial class ListLocalRecipeViewModel : ListRecipeViewModel
{
    private readonly ILocalRecipeListService _localRecipeListService;
    private readonly IGetLocalRecipeService _getLocalRecipeService;
    
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(OpenRecipeCommand))]
    private RecipeEntry? _selectedRecipeEntry;
    
    private bool RecipeSelected => SelectedRecipeEntry != null;
    
    public ListLocalRecipeViewModel(ILocalRecipeListService recipeListService, IGetLocalRecipeService getLocalRecipeService, string appDataPath) : base(recipeListService, appDataPath)
    {
        _localRecipeListService = recipeListService;
        _getLocalRecipeService = getLocalRecipeService;
        _ = RefreshAvailableFilterOptions();
    }
    
    private async Task RefreshAvailableFilterOptions()
    {
        AvailableFilterOptions = (await _localRecipeListService.GetCategories(FilterOptionOrderBy.TITLE, Order.ASCENDING, 10, 0)).Select(x => x.Name).ToList();
    }
    
    [RelayCommand(CanExecute = nameof(RecipeSelected))]
    private async Task OpenRecipe()
    {
        Recipe curRecipe = await _getLocalRecipeService.GetRecipe(SelectedRecipeEntry!.Hash);
        var navigationParameter = new Dictionary<string, object> { { "Recipe", curRecipe } };
        await Shell.Current.GoToAsync($"{nameof(ShowRecipeView)}", navigationParameter);
    }

    [RelayCommand]
    private async Task OpenRecipesFolder()
    {
        #if WINDOWS
        Process.Start("explorer.exe", AppDataPath);
        #elif MACCATALYST
        Process.Start("open", AppDataPath);
        #endif
    }
}