using System.Collections.ObjectModel;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GUI.ViewModel;

public partial class ListRemoteRecipeViewModel : ListRecipeViewModel
{
    private readonly IOnlineRecipeListService _recipeListService;
    private readonly IDownloadRecipeService _downloadRecipeService;
    
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(DownloadRecipeCommand))]
    private RecipeEntry? _selectedRecipeEntry;
    
    private bool RecipeSelected => SelectedRecipeEntry != null;
    
    public ListRemoteRecipeViewModel(IOnlineRecipeListService recipeListService, IDownloadRecipeService downloadService, string appDataPath) : base(recipeListService, appDataPath)
    {
        _recipeListService = recipeListService;
        _downloadRecipeService = downloadService;
        _ = RefreshAvailableFilterOptions();
    }

    private async Task RefreshAvailableFilterOptions()
    {
        AvailableFilterOptions = await _recipeListService.GetCategories(10, 0);
    }

    [RelayCommand(CanExecute = nameof(RecipeSelected))]
    private async Task DownloadRecipe()
    {
        await _downloadRecipeService.DownloadRecipe(SelectedRecipeEntry!.Hash);
    }
}